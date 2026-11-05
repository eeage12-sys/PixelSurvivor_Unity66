using UnityEngine;

namespace PixelSurvivor
{
    // The five 4x4 sheets share row order: down, up, left, right.
    public static class RedesignedSprites
    {
        static readonly Sprite[][][] cache = new Sprite[5][][];
        static Sprite[] monsters;
        static Sprite elite;

        public static Sprite Elite()
        {
            if (elite != null) return elite;
            var texture = Resources.Load<Texture2D>("Redesign/Elite");
            if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            elite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, .5f), 1100f, 0, SpriteMeshType.FullRect);
            elite.name = "Redesign_Elite";
            return elite;
        }

        public static Sprite Monster(int kind)
        {
            if (kind < 0 || kind > 3) return null;
            if (monsters == null)
            {
                var texture = Resources.Load<Texture2D>("Redesign/Monsters");
                if (texture == null) return null;
                texture.filterMode = FilterMode.Point;
                var created = new Sprite[4];
                // The generated silhouettes have different widths; fixed half-sheet cells
                // would cut the moth's wings into the slime sprite.
                var rects = new[]
                {
                    new Rect(0, 760, 510, 390),
                    new Rect(510, 760, 759, 480),
                    new Rect(0, 0, 510, 760),
                    new Rect(510, 0, 759, 760)
                };
                var pixelsPerUnit = new[] { 380f, 700f, 600f, 750f };
                float scaleX = texture.width / 1269f, scaleY = texture.height / 1240f;
                for (int i = 0; i < created.Length; i++)
                {
                    var area = rects[i];
                    var scaled = new Rect(area.x * scaleX, area.y * scaleY, area.width * scaleX, area.height * scaleY);
                    created[i] = Sprite.Create(texture, scaled,
                        new Vector2(.5f, .5f), pixelsPerUnit[i] * scaleX, 0, SpriteMeshType.FullRect);
                    created[i].name = "Redesign_Monster_" + i;
                }
                monsters = created;
            }
            return monsters[kind];
        }

        public static Sprite[] Direction(int character, int direction)
        {
            if (character < 0 || character >= cache.Length || direction < 0 || direction > 3) return null;
            if (cache[character] == null)
            {
                var texture = Resources.Load<Texture2D>("Redesign/Character" + character);
                if (texture == null) return null;
                texture.filterMode = FilterMode.Point;
                var directions = new Sprite[4][];
                for (int row = 0; row < 4; row++)
                {
                    directions[row] = new Sprite[4];
                    for (int column = 0; column < 4; column++)
                    {
                        int left = column * texture.width / 4;
                        int right = (column + 1) * texture.width / 4;
                        int bottom = (3 - row) * texture.height / 4;
                        int top = (4 - row) * texture.height / 4;
                        var rect = new Rect(left, bottom, right - left, top - bottom);
                        directions[row][column] = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 280f, 0, SpriteMeshType.FullRect);
                        directions[row][column].name = "Redesign_Character" + character + "_" + row + "_" + column;
                    }
                }
                cache[character] = directions;
            }
            return cache[character][direction];
        }
    }
}
