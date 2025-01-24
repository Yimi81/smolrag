import os
import argparse
from smolrag import SmolRAG
from smolrag.utils import EmbeddingFunc
from transformers import AutoModel,AutoTokenizer
from smolrag.llm import openai_embedding

EMBEDDING_MODEL = "sentence-transformers/all-MiniLM-L6-v2"

def get_args(): 
    parser = argparse.ArgumentParser(description="MiniRAG")
    parser.add_argument('--model', type=str, default='PHI')
    parser.add_argument('--outputpath', type=str, default='./logs/Default_output.csv')
    parser.add_argument('--workingdir', type=str, default='./ZTResource')
    parser.add_argument('--datapath', type=str, default='./Assets/ZTResource/Scripts/')
    parser.add_argument('--querypath', type=str, default='./dataset/LiHua-World/qa/query_set.csv')
    args = parser.parse_args()
    return args

args = get_args()

if args.model == 'PHI':
    LLM_MODEL = "microsoft/Phi-3.5-mini-instruct"
elif args.model == 'GLM':
    LLM_MODEL = "THUDM/glm-edge-1.5b-chat"
elif args.model == 'MiniCPM':
    LLM_MODEL = "openbmb/MiniCPM3-4B"
elif args.model == 'qwen':
    LLM_MODEL = "Qwen/Qwen2.5-3B-Instruct"
else:
    print("Invalid model name")
    exit(1)

WORKING_DIR = args.workingdir
DATA_PATH = args.datapath
QUERY_PATH = args.querypath
OUTPUT_PATH = args.outputpath
print("USING LLM:", LLM_MODEL)
print("USING WORKING DIR:", WORKING_DIR)

if not os.path.exists(WORKING_DIR):
    os.mkdir(WORKING_DIR)


rag = SmolRAG(
    working_dir=WORKING_DIR,
    llm_model_max_token_size=200,
    embedding_func=EmbeddingFunc(
        embedding_dim=1024,
        max_token_size=8192,
        func=lambda texts: openai_embedding(
            texts, 
            model="bge-m3",
            base_url="http://10.1.2.119:9997/v1/",
            api_key="test",
        )
    ),
)

#Now indexing
def find_txt_files(root_path):
    txt_files = []
    for root, dirs, files in os.walk(root_path):
        for file in files:
            if file.endswith('.txt'):
                txt_files.append(os.path.join(root, file))
    return txt_files

WEEK_LIST = find_txt_files(DATA_PATH)
for WEEK in WEEK_LIST:
    id = WEEK_LIST.index(WEEK)
    print(f"{id}/{len(WEEK_LIST)}")
    with open(WEEK) as f:
        rag.insert(f.read())
