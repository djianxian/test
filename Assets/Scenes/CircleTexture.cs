using UnityEngine;
using System.IO;

public class CircleTexture : MonoBehaviour
{
    public int textureWidth = 256;  // 图片宽度
    public int textureHeight = 256; // 图片高度

    void Start()
    {
        // 创建一个新的纹理
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        
        // 设置纹理的每个像素
        for (int x = 0; x < textureWidth; x++)
        {
            for (int y = 0; y < textureHeight; y++)
            {
                // 计算该点距离图片中心的距离
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(textureWidth / 2, textureHeight / 2));
                
                // 如果距离小于半径，设置为红色
                if (dist <= textureWidth / 2)
                {
                    texture.SetPixel(x, y, Color.red);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);  // 设置透明
                }
            }
        }

        // 应用像素设置
        texture.Apply();

        // 保存为 PNG 文件
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/circle.png", bytes);

        // 在控制台打印路径
        Debug.Log("PNG saved at: " + Application.dataPath + "/circle.png");
    }
}