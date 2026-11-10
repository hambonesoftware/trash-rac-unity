using System.Collections.Generic;
using UnityEngine;

namespace TrashRac
{
    internal sealed partial class AlleyArt
    {
        private void MakeDoorSpill()
        {
            Transform spill = Cutout("Amber light on pavement", DoorSpill, Vector3.zero, "8e5767",
                new(-0.80f, 1.37f), new(0.80f, 1.37f), new(1.74f, -2.45f), new(-1.53f, -2.45f));
            spill.localRotation = Quaternion.Euler(90, 0, 0);
            Transform core = Cutout("Bright edge of spill", DoorSpill, new Vector3(0, 0.003f, 0), "ca856e",
                new(-0.72f, 1.35f), new(0.72f, 1.35f), new(0.98f, -0.76f), new(-0.86f, -0.76f));
            core.localRotation = spill.localRotation;
        }

        private void AddComicScenery(Transform root, Transform market, Transform dumpster, Transform skyline)
        {
            var haze = Group("Bands of city haze", new Vector3(0, 0, 18), skyline);
            Cube("Midnight sky", haze, new Vector3(0, 3.0f, 0), new Vector3(47, 14, 0.06f), "252348", 0, 0.85f);
            Cube("Lower haze", haze, new Vector3(0, 0.61f, -0.04f), new Vector3(47, 4.4f, 0.022f), "443157", 0, 0.82f);
            Cutout("Low cloud", skyline, new Vector3(5.62f, 3.89f, 14.5f), "564164",
                new(-4.5f, -0.08f), new(-2.6f, 0.22f), new(-0.46f, 0.21f), new(0.84f, 0.05f),
                new(3.88f, 0.02f), new(1.85f, -0.17f), new(-2.2f, -0.18f));
            Cutout("High cloud", skyline, new Vector3(4.67f, 4.61f, 16.8f), "423353",
                new(-3.45f, -0.03f), new(-1.86f, 0.15f), new(1.05f, 0.14f), new(1.94f, 0.04f), new(-0.28f, -0.13f));
            Cutout("Moon halo", skyline, new Vector3(8.4f, 4.65f, 14.8f), "6b5372",
                new(-0.96f, 0.04f), new(-0.62f, 0.69f), new(0, 0.91f), new(0.65f, 0.62f),
                new(0.92f, 0.02f), new(0.63f, -0.63f), new(-0.03f, -0.9f), new(-0.69f, -0.59f));

            Cube("Side wall return", market, new Vector3(-1.43f, 3.41f, 4.41f),
                new Vector3(0.24f, 6.82f, 3.61f), "392950", 0.023f);
            Cube("Building edge", market, new Vector3(-1.48f, 3.48f, 2.31f),
                new Vector3(0.20f, 6.96f, 0.29f), "725079", 0.014f);
            for (int i = 0; i < 21; i++)
            {
                float x = -10.8f + i % 7 * 1.23f + i % 2 * 0.31f;
                float y = 0.75f + i / 7 * 1.48f + (i % 3) * 0.49f;
                Cube("Warm brick", market, new Vector3(x, y, 2.386f),
                    new Vector3(0.65f + i % 3 * 0.18f, 0.25f, 0.019f), i % 3 == 0 ? "644675" : "58406b", 0);
            }

            Transform pipe = Group("Service pipe", new Vector3(-9.03f, 0, 2.11f), market);
            Cube("Pipe", pipe, new Vector3(0, 3.36f, 0), new Vector3(0.16f, 4.38f, 0.16f), "342b4d", 0.017f);
            Cube("Pipe return", pipe, new Vector3(0.25f, 1.19f, 0), new Vector3(0.62f, 0.16f, 0.16f), "584562");
            Cube("Pipe drop", pipe, new Vector3(0.52f, 0.63f, 0), new Vector3(0.16f, 1.19f, 0.16f), "584562");
            foreach (float y in new[] { 1.28f, 3.22f, 4.23f, 5.28f })
                Cube("Pipe clamp", pipe, new Vector3(0, y, -0.013f), new Vector3(0.25f, 0.069f, 0.22f), "93758d", 0.006f);
            Cube("Sign light", market, new Vector3(-7.5f, 5.41f, 1.99f),
                new Vector3(2.4f, 0.067f, 0.065f), "efa271", 0, 1);
            Text(market, "GROCERIES  /  EST. 1987", new Vector3(-7.50f, 4.56f, 2.031f), 0.111f, "d99485");

            for (int i = 0; i < 5; i++)
            {
                Cube("Dumpster lid rib", dumpster, new Vector3(-1.5f + i * 0.75f, 2.08f, -0.02f),
                    new Vector3(0.12f, 0.037f, 1.79f), "5b7b83", 0.006f)
                    .localRotation = Quaternion.Euler(-9, 0, 0);
            }
            Cube("Front rim", dumpster, new Vector3(0, 1.83f, -0.96f),
                new Vector3(4.37f, 0.12f, 0.09f), "63a49b", 0.008f);
            Cube("Bottom rim", dumpster, new Vector3(0, 0.18f, -0.95f),
                new Vector3(4.15f, 0.12f, 0.09f), "1d6060");
            foreach (float x in new[] { -1.69f, 1.68f })
                Ball("Dumpster caster", dumpster, new Vector3(x, 0.15f, -0.63f), Vector3.one * 0.31f, "2b2742");
            for (int i = 0; i < 4; i++)
                Cube("Safety tape", dumpster, new Vector3(0.81f + i * 0.12f, 1.54f, -0.954f),
                    new Vector3(0.055f, 0.16f, 0.015f), "e9b375", 0, 0.48f)
                    .localRotation = Quaternion.Euler(0, 0, -26);

            GroundArt("Puddle reflection", root, new Vector3(-1.77f, 0.008f, -3.13f), "53426d",
                new(-1.48f, 0.15f), new(-0.8f, 0.34f), new(0.3f, 0.3f), new(1.48f, -0.04f),
                new(0.9f, -0.33f), new(-0.3f, -0.36f));
            GroundArt("Puddle gleam", root, new Vector3(-1.77f, 0.012f, -3.13f), "7a598a",
                new(-0.89f, 0.05f), new(0.39f, 0.07f), new(0.87f, -0.08f), new(-0.37f, -0.05f));
            GroundArt("Alley seam", root, new Vector3(2.24f, 0.007f, -3.8f), "302548",
                new(-5, 0.16f), new(6, -0.19f), new(6, -0.23f), new(-5, 0.11f));

            BatchFlatScenery(root);
        }

        private void GroundArt(string name, Transform root, Vector3 position, string color, params Vector2[] points)
        {
            Cutout(name, root, position, color, points).localRotation = Quaternion.Euler(90, 0, 0);
        }

        private void BatchFlatScenery(Transform root)
        {
            var batches = new Dictionary<Material, List<CombineInstance>>();
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>())
            {
                Transform item = filter.transform;
                Mesh mesh = filter.sharedMesh;
                if (!mesh || !mesh.isReadable || item.IsChildOf(Door) || item.IsChildOf(DoorSpill)
                    || item.IsChildOf(Can) || item == DoorLight)
                    continue;

                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
                Material material = renderer.sharedMaterial;
                if (material.GetFloat("_Outline") > 0) continue;
                if (!batches.TryGetValue(material, out List<CombineInstance> group))
                    batches.Add(material, group = new List<CombineInstance>());
                group.Add(new CombineInstance { mesh = mesh, transform = root.worldToLocalMatrix * item.localToWorldMatrix });
                renderer.enabled = false;
            }

            foreach (var batch in batches)
            {
                var mesh = new Mesh { name = "Batched flat scenery" };
                mesh.CombineMeshes(batch.Value.ToArray());
                Transform combined = Group("Scenery batch", Vector3.zero, root);
                combined.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                combined.gameObject.AddComponent<MeshRenderer>().sharedMaterial = batch.Key;
            }
        }
    }
}
