import os
from dataclasses import asdict, dataclass, field
from datetime import datetime
from typing import Type, cast
from .utils import EmbeddingFunc, logger, set_logger, limit_async_func_call
from .llm import *

from .base import (
    BaseKVStorage,
    BaseVectorStorage,
    StorageNameSpace,
)

from .storage import (
    JsonKVStorage,
    NanoVectorDBStorage,
)


@dataclass
class SmolRAG:
    working_dir: str = field(
        default_factory=lambda: f"./smolrag_cache_{datetime.now().strftime('%Y-%m-%d-%H:%M:%S')}"
    )

    embedding_func: EmbeddingFunc = field(default_factory=lambda: openai_embedding)
    embedding_batch_num: int = 32
    embedding_func_max_async: int = 16

    current_log_level = logger.level
    log_level: str = field(default=current_log_level)

    def __post_init__(self):
        set_logger(os.path.join(self.working_dir, "smolrag.log"))
        logger.setLevel(self.log_level)

        logger.info(f"Logger initialized for working directory: {self.working_dir}")

        _print_config = ",\n  ".join([f"{k} = {v}" for k, v in asdict(self).items()])
        logger.debug(f"SmolRAG init with param:\n  {_print_config}\n")

        if not os.path.exists(self.working_dir):
            logger.info(f"Creating working directory {self.working_dir}")
            os.makedirs(self.working_dir)

        self.embedding_func = limit_async_func_call(self.embedding_func_max_async)(
            self.embedding_func
        )

    def _get_storage_class(self) -> Type[StorageNameSpace]:
        return {
            # kv storage
            "JsonKVStorage": JsonKVStorage,
            # vector storage
            "NanoVectorDBStorage": NanoVectorDBStorage,
            # @TODO graph storage
            # @TODO "ArangoDBStorage": ArangoDBStorage
        }
