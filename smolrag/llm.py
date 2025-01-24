import os
import torch
import copy
import numpy as np
from functools import lru_cache
from openai import (
    AsyncOpenAI,
    APIConnectionError,
    RateLimitError,
    Timeout,
    AsyncAzureOpenAI,
)

from tenacity import (
    retry,
    stop_after_attempt,
    wait_exponential,
    retry_if_exception_type,
)
from transformers import AutoTokenizer, AutoModelForCausalLM

from .utils import wrap_embedding_func_with_attrs, compute_args_hash
from .base import BaseKVStorage


@lru_cache(maxsize=1)
def initialize_hf_model(model_name):
    hf_tokenizer = AutoTokenizer.from_pretrained(
        model_name, device_map="auto", trust_remote_code=True  # False
    )
    hf_model = AutoModelForCausalLM.from_pretrained(
        model_name, device_map="auto", trust_remote_code=True
    )
    if hf_tokenizer.pad_token is None:
        hf_tokenizer.pad_token = hf_tokenizer.eos_token

    return hf_model, hf_tokenizer


async def hf_model_if_cache(
    model, prompt, system_prompt=None, history_messages=[], **kwargs
) -> str:
    model_name = model
    hf_model, hf_tokenizer = initialize_hf_model(model_name)
    hashing_kv: BaseKVStorage = kwargs.pop("hashing_kv", None)
    messages = []
    if system_prompt:
        messages.append({"role": "system", "content": system_prompt})
    messages.extend(history_messages)
    messages.append({"role": "user", "content": prompt})

    if hashing_kv is not None:
        args_hash = compute_args_hash(model, messages)
        if_cache_return = await hashing_kv.get_by_id(args_hash)
        if if_cache_return is not None:
            return if_cache_return["return"]
    input_prompt = ""
    try:
        input_prompt = hf_tokenizer.apply_chat_template(
            messages, tokenize=False, add_generation_prompt=True
        )
    except Exception:
        try:
            ori_message = copy.deepcopy(messages)
            if messages[0]["role"] == "system":
                messages[1]["content"] = (
                    "<system>"
                    + messages[0]["content"]
                    + "</system>\n"
                    + messages[1]["content"]
                )
                messages = messages[1:]
                input_prompt = hf_tokenizer.apply_chat_template(
                    messages, tokenize=False, add_generation_prompt=True
                )
        except Exception:
            len_message = len(ori_message)
            for msgid in range(len_message):
                input_prompt = (
                    input_prompt
                    + "<"
                    + ori_message[msgid]["role"]
                    + ">"
                    + ori_message[msgid]["content"]
                    + "</"
                    + ori_message[msgid]["role"]
                    + ">\n"
                )

    input_ids = hf_tokenizer(
        input_prompt, return_tensors="pt", padding=True, truncation=True
    ).to("cuda")
    torch.cuda.empty_cache()
    # inputs = {k: v.to(hf_model.device) for k, v in input_ids.items()}
    output = hf_model.generate(
        **input_ids, max_new_tokens=500, num_return_sequences=1, early_stopping=True
    )
    response_text = hf_tokenizer.decode(
        output[0][len(input_ids[0]) :], skip_special_tokens=True
    )

    FINDSTRING = "<|COMPLETE|>"
    last_assistant_index = response_text.find(FINDSTRING)

    if last_assistant_index != -1:
        response_text = response_text[: last_assistant_index + len(FINDSTRING)]
    else:
        response_text = response_text

    if hashing_kv is not None:
        await hashing_kv.upsert({args_hash: {"return": response_text, "model": model}})

    return response_text


async def hf_model_complete(
    prompt, system_prompt=None, history_messages=[], **kwargs
) -> str:
    model_name = kwargs["hashing_kv"].global_config["llm_model_name"]
    return await hf_model_if_cache(
        model_name,
        prompt,
        system_prompt=system_prompt,
        history_messages=history_messages,
        **kwargs,
    )


@wrap_embedding_func_with_attrs(embedding_dim=1536, max_token_size=8192)
@retry(
    stop=stop_after_attempt(3),
    wait=wait_exponential(multiplier=1, min=4, max=60),
    retry=retry_if_exception_type((RateLimitError, APIConnectionError, Timeout)),
)
async def openai_embedding(
    texts: list[str],
    model: str = "text-embedding-3-small",
    base_url: str = None,
    api_key: str = None,
) -> np.ndarray:
    if api_key:
        os.environ["OPENAI_API_KEY"] = api_key

    openai_async_client = (
        AsyncOpenAI() if base_url is None else AsyncOpenAI(base_url=base_url)
    )
    response = await openai_async_client.embeddings.create(
        model=model, input=texts, encoding_format="float"
    )
    return np.array([dp.embedding for dp in response.data])
