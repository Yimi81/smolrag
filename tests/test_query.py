import os
import csv
import argparse
from smolrag import SmolRAG, QueryParam
from smolrag.utils import EmbeddingFunc
from transformers import AutoModel,AutoTokenizer
from smolrag.llm import openai_embedding, hf_model_complete
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
    llm_model_func=hf_model_complete,
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


def run_experiment(output_path):
    
    q_already = []
    if os.path.exists(output_path):
        with open(output_path, mode='r', encoding='utf-8') as question_file:
            reader = csv.DictReader(question_file)
            for row in reader:
                q_already.append(row['Question'])

    row_count = len(q_already)
    print('row_count', row_count)

    with open(output_path, mode='a', newline='', encoding='utf-8') as log_file:
        writer = csv.writer(log_file)
        QUESTION = "项目中是如何实现三视图截取的？"
        try:
            minirag_answer = rag.query(QUESTION, param=QueryParam(mode="naive")).replace("\n", "").replace("\r", "")
        except Exception as e:
            print('Error in minirag_answer', e)

        writer.writerow([QUESTION, "None",minirag_answer])

    print(f'Experiment data has been recorded in the file: {output_path}')

# if __name__ == "__main__":

run_experiment(OUTPUT_PATH)