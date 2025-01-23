using UnityEngine;

// 定义 ITagLibrary 接口，确保在运行时和编辑器中都可以使用
public interface ITagLibrary
{
    string[] Tags { get; }
}