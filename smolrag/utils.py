import asyncio
import json
import os
import logging
import numpy as np

import pandas as pd
from openai import APIStatusError, AsyncOpenAI, BadRequestError
from .prompts import describe_code_prompt

from functools import wraps
from dataclasses import dataclass
from hashlib import md5

logger = logging.getLogger("smolrag")


def set_logger(log_file: str):
    logger.setLevel(logging.DEBUG)

    file_handler = logging.FileHandler(log_file)
    file_handler.setLevel(logging.DEBUG)

    formatter = logging.Formatter(
        "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
    )
    file_handler.setFormatter(formatter)

    if not logger.handlers:
        logger.addHandler(file_handler)


def limit_async_func_call(max_size: int, waitting_time: float = 0.0001):
    """Add restriction of maximum async calling times for a async func"""

    def final_decro(func):
        """Not using async.Semaphore to aovid use nest-asyncio"""
        __current_size = 0

        @wraps(func)
        async def wait_func(*args, **kwargs):
            nonlocal __current_size
            while __current_size >= max_size:
                await asyncio.sleep(waitting_time)
            __current_size += 1
            result = await func(*args, **kwargs)
            __current_size -= 1
            return result

        return wait_func

    return final_decro


@dataclass
class EmbeddingFunc:
    embedding_dim: int
    max_token_size: int
    func: callable

    async def __call__(self, *args, **kwargs) -> np.ndarray:
        return await self.func(*args, **kwargs)


def wrap_embedding_func_with_attrs(**kwargs):
    """Wrap a function with attributes"""

    def final_decro(func) -> EmbeddingFunc:
        new_func = EmbeddingFunc(**kwargs, func=func)
        return new_func

    return final_decro


def load_json(file_name):
    if not os.path.exists(file_name):
        return None
    with open(file_name, encoding="utf-8") as f:
        return json.load(f)


def write_json(json_obj, file_name):
    with open(file_name, "w", encoding="utf-8") as f:
        json.dump(json_obj, f, indent=2, ensure_ascii=False)

def compute_mdhash_id(content, prefix: str = ""):
    return prefix + md5(content.encode()).hexdigest()

def load_data():
    data = pd.read_csv("data/zt_resource.csv", encoding="utf-8")
    return data


async def process_asset(
    client,
    prompt,
    semaphore,
    id,
    code,
):
    async with semaphore:
        messages = [
            {"role": "user", "content": prompt.format(code=code)},
        ]
        try:
            response = await client.chat.completions.create(
                model="deepseek-r1-qwen-32b", messages=messages
            )

            if response:
                description = response.choices[0].message.content
                print(description)

            result = {id: description}
            return result

        except (BadRequestError, APIStatusError) as e:
            print(f"Skipping asset {id} due to content filter violation: {e}")
            return {id: None}
        except Exception as e:
            print(f"An error occurred while processing asset {id}: {e}")
            return {id: None}


async def tag_scripts(data):
    """自动对房间布局进行打标, 获取该房间的描述

    Args:
        yolo_data (dict): 房间布局yolo数据
    """
    client = AsyncOpenAI(
        api_key="test",
        base_url=f"http://10.3.2.203:8000/v1",
    )

    if data is not None:
        semaphore = asyncio.Semaphore(10)  # Adjust the number to control concurrency
        tasks = []

        for idx, row in data.iterrows():
            id = row["ID"]
            script_path = row["Path"]
            try:
                with open(script_path, "r", encoding="utf-8") as file:
                    code = file.read()
            except UnicodeDecodeError:
                with open(script_path, "r", encoding="latin1") as file:
                    code = file.read()

            tasks.append(
                process_asset(
                    client,
                    describe_code_prompt,
                    semaphore,
                    id,
                    code,
                )
            )

        results = await asyncio.gather(*tasks)

        room_description_map = {
            list(res.keys())[0]: res[list(res.keys())[0]]
            for res in results
            if res and list(res.values())[0] is not None
        }

        for id, description in room_description_map.items():
            data.loc[data["ID"] == id, "DetailDesc"] = description

    data.to_csv("data/zt_resource_v2.csv", index=False, encoding="utf-8")
    return data


if __name__ == "__main__":

    data = load_data()

    asyncio.run(tag_scripts(data))
