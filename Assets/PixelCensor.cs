using UnityEngine;

public class PixelCensor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Texture2D censorTexture;

    [SerializeField] private int textureSize = 64;       // Total storlek (64x64)
    [SerializeField] private int pixelBlockSize = 8;     // Storleken på varje pixel-block (t.ex. 8x8 stora block)
    [SerializeField] private float flickerSpeed = 0.05f; // Hur snabbt det flimrar

    private float timer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Skapa en ny tom textur
        censorTexture = new Texture2D(textureSize, textureSize);
        censorTexture.filterMode = FilterMode.Point; // Håller pixlarna skarpa och "retro"
        censorTexture.wrapMode = TextureWrapMode.Clamp;

        // Generera första bildrutan
        GeneratePixelNoise();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= flickerSpeed)
        {
            GeneratePixelNoise();
            timer = 0f;
        }
    }

    void GeneratePixelNoise()
    {
        // Loopa igenom texturen i block (t.ex. steg om 8 pixlar)
        for (int y = 0; y < textureSize; y += pixelBlockSize)
        {
            for (int x = 0; x < textureSize; x += pixelBlockSize)
            {
                // Välj en slumpmässig grå/svart färg för detta block
                float gray = Random.Range(0.1f, 0.5f);
                Color blockColor = new Color(gray, gray, gray, 1f);

                // Färglägg hela blocket (t.ex. 8x8 pixlar) med samma färg
                for (int blockY = 0; blockY < pixelBlockSize; blockY++)
                {
                    for (int blockX = 0; blockX < pixelBlockSize; blockX++)
                    {
                        censorTexture.SetPixel(x + blockX, y + blockY, blockColor);
                    }
                }
            }
        }

        // Applicera ändringarna på texturen
        censorTexture.Apply();

        // Skapa en Sprite av texturen och lägg på SpriteRenderern
        spriteRenderer.sprite = Sprite.Create(censorTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
    }
}
