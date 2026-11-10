using UnityEngine;

namespace TrashRac
{
    internal sealed class AlleyEffects
    {
        internal enum Cue { Stack, Cover, Hit, Duck, Spotted }

        private sealed class Stamp
        {
            public Transform Art;
            public Vector3 Origin;
            public float Age;
            public float Duration;
        }

        private readonly Stamp[] stamps = new Stamp[5];
        private readonly Transform[] chips = new Transform[15];
        private readonly Vector3[] chipVelocity = new Vector3[15];
        private readonly System.Random random = new(1023);
        private float chipAge = 1;
        private int activeChipCount;

        public AlleyEffects(AlleyArt art, Transform root)
        {
            MakeStamp(art, root, Cue.Stack, "KRAK!", "ffb159", "fff5d2", 0.63f, 14, 0.82f);
            MakeStamp(art, root, Cue.Cover, "CLANG!", "8ce0d9", "f5ffff", 0.75f, 12, 0.89f);
            MakeStamp(art, root, Cue.Hit, "BONK!", "f1858c", "fff4d8", 0.67f, 12, 0.78f);
            MakeStamp(art, root, Cue.Duck, "SHH", "a6c997", "effaca", 0.40f, 0, 0.47f);
            MakeStamp(art, root, Cue.Spotted, "!", "ffba69", "fff2c8", 0.73f, 8, 0.43f);
            for (int i = 0; i < chips.Length; i++)
            {
                bool dust = i % 3 == 0;
                chips[i] = dust
                    ? art.Part(PrimitiveType.Sphere, "Dust puff", root, Vector3.zero, Vector3.one, "a18a9f", 0, 0.7f)
                    : art.Cutout("Cardboard chip", root, Vector3.zero, i % 2 == 0 ? "e4ac75" : "bd815f",
                        new(-0.15f, 0.055f), new(0.11f, 0.125f), new(0.075f, -0.07f), new(-0.04f, -0.11f));
                chips[i].gameObject.SetActive(false);
            }
        }

        private void MakeStamp(AlleyArt art, Transform root, Cue cue, string word, string accent, string letters,
            float duration, int rays, float size)
        {
            Transform stamp = AlleyArt.Group(word, Vector3.zero, root);
            if (rays > 0)
            {
                var corners = new Vector2[rays * 2];
                for (int i = 0; i < corners.Length; i++)
                {
                    float angle = Mathf.PI * 2 * i / corners.Length;
                    float radius = i % 2 == 0 ? 1 + i % 3 * 0.11f : 0.65f;
                    corners[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * 0.69f);
                }
                art.Cutout("Ink burst", stamp, Vector3.zero, "241835", corners);
                for (int i = 0; i < corners.Length; i++) corners[i] *= 0.83f;
                art.Cutout("Color burst", stamp, new Vector3(0, 0, -0.013f), accent, corners);
            }
            TextMesh text = AlleyArt.Text(stamp, word, new Vector3(0, 0, -0.027f), size * 0.51f, letters);
            text.fontStyle = FontStyle.BoldAndItalic;
            stamp.gameObject.SetActive(false);
            stamps[(int)cue] = new Stamp { Art = stamp, Duration = duration };
        }

        public void Pop(Cue cue, Vector3 position)
        {
            Stamp stamp = stamps[(int)cue];
            stamp.Origin = position;
            stamp.Age = 0;
            stamp.Art.position = position;
            stamp.Art.localScale = Vector3.one * (cue == Cue.Spotted ? 0.48f : 0.72f);
            stamp.Art.gameObject.SetActive(true);
        }

        public void Scatter(Vector3 position, int cartons)
        {
            chipAge = 0;
            activeChipCount = Mathf.Min(chips.Length, 4 + cartons * 3);
            for (int i = 0; i < chips.Length; i++)
            {
                Transform chip = chips[i];
                chip.position = position + new Vector3(Next(-0.32f, 0.32f), Next(-0.25f, 0.25f), -0.6f);
                chip.localScale = Vector3.one * (i % 3 == 0 ? 0.24f : Next(0.55f, 1.05f));
                chip.localRotation = Quaternion.Euler(0, 0, Next(-180, 180));
                chipVelocity[i] = new Vector3(Next(-2.6f, 3.15f), Next(0.9f, 3.0f), Next(-0.8f, 0.3f));
                chip.gameObject.SetActive(i < activeChipCount);
            }
        }

        private float Next(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());

        public void Update(float dt)
        {
            for (int i = 0; i < stamps.Length; i++)
            {
                Stamp stamp = stamps[i];
                if (!stamp.Art.gameObject.activeSelf) continue;
                stamp.Age += dt;
                float t = Mathf.Clamp01(stamp.Age / stamp.Duration);
                float zoom = t < 0.22f ? Mathf.Lerp(0.53f, 1.08f, Mathf.SmoothStep(0, 1, t / 0.22f))
                    : Mathf.Lerp(1.08f, 0, Mathf.SmoothStep(0, 1, (t - 0.58f) / 0.42f));
                stamp.Art.localScale = Vector3.one * zoom * (i == (int)Cue.Spotted ? 0.54f : 1);
                stamp.Art.position = stamp.Origin + Vector3.up * (t * 0.47f);
                stamp.Art.localRotation = Quaternion.Euler(-8, 0, i % 2 == 0 ? -8 : 8);
                if (t >= 1) stamp.Art.gameObject.SetActive(false);
            }

            if (chipAge >= 0.72f) return;
            chipAge += dt;
            for (int i = 0; i < activeChipCount; i++)
            {
                Transform chip = chips[i];
                bool dust = i % 3 == 0;
                chipVelocity[i].y -= (dust ? 2.3f : 7.5f) * dt;
                chip.position += chipVelocity[i] * dt;
                chip.Rotate(0, dust ? 0 : 90 * dt, 140 * dt);
                float size = dust ? 0.15f + Mathf.Sin(chipAge / 0.72f * Mathf.PI) * 0.51f
                    : Mathf.Clamp01((0.72f - chipAge) * 4.6f);
                chip.localScale = Vector3.one * size;
                if (chipAge >= 0.72f || chip.position.y < 0) chip.gameObject.SetActive(false);
            }
        }

        public void Clear()
        {
            foreach (Stamp stamp in stamps) stamp.Art.gameObject.SetActive(false);
            foreach (Transform chip in chips) chip.gameObject.SetActive(false);
            chipAge = 1;
        }
    }
}
