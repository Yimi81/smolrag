import pandas as pd
import os
import re

# 读取 CSV 文件
df = pd.read_csv('data/zt_resource_v2.csv')

# 遍历每一行
for index, row in df.iterrows():
    id_ = row['ID']
    path = row['Path']
    detail_desc = row['DetailDesc']
    
    # 读取源代码内容
    if os.path.exists(path):
        try:
            with open(path, 'r', encoding='utf-8') as file:
                source_code = file.read()
        except UnicodeDecodeError:
            with open(path, 'r', encoding='latin1') as file:
                source_code = file.read()
    else:
        source_code = "// 源代码文件未找到。\n"
    
    # 删除 <think> 和 </think> 之间的内容
    explanation = re.sub(r'<think>.*?</think>', '', detail_desc, flags=re.DOTALL).strip()
    
    # 构造 txt 文件内容
    txt_content = f"文件源代码内容如下：\n{source_code}\n源代码内容解释如下：\n{explanation}"
    
    directory = os.path.dirname(path)
    
    # 确保目录存在
    os.makedirs(directory, exist_ok=True)
    
    # 保存为 TXT 文件
    txt_filename = os.path.join(directory, f"{id_}.txt")
    with open(txt_filename, 'w', encoding='utf-8') as f:
        f.write(txt_content)