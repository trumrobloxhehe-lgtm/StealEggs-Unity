using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public static class StealEggsFoliage
{
    public static void Build(Transform root, Vector3 worldPos, Material trunkMat, Material leafMat, float scale)
    {
        int kind = 0;
        if (worldPos.x > 8f && worldPos.x < 42f && Mathf.Abs(worldPos.z) > 16f && Random.value < .62f) kind = 2;
        else if (worldPos.x < 10f ? Random.value < .72f : Random.value < .38f) kind = 1;

        Color leaf = leafMat != null ? leafMat.color : new Color(.16f, .42f, .18f);
        if (kind == 1) leaf = Color.Lerp(new Color(.07f, .28f, .12f), leaf, .35f);
        if (kind == 2) leaf = Color.Lerp(new Color(.10f, .46f, .16f), leaf, .25f);

        Material bark = trunkMat;
        Material barkLight = StealEggsDemoSetupMaterials.Get("BarkHighlight", new Color(.46f, .28f, .14f));
        root.name = kind == 1 ? "Pine Tree" : kind == 2 ? "Palm Tree" : "Broadleaf Tree";
        root.localScale = Vector3.one * scale;
        float lean = Random.Range(-3.5f, 3.5f);

        Material canopy = CardMat(kind == 1 ? "PineNeedles" : kind == 2 ? "PalmFronds" : "BroadleafCanopy", leaf, kind);
        Material canopyLight = CardMat(kind == 1 ? "PineNeedlesSun" : kind == 2 ? "PalmFrondsSun" : "BroadleafCanopySun", Color.Lerp(leaf, new Color(.78f, .86f, .42f), .38f), kind);
        Material canopyDark = CardMat(kind == 1 ? "PineNeedlesShade" : kind == 2 ? "PalmFrondsShade" : "BroadleafCanopyShade", Color.Lerp(leaf, new Color(.03f, .08f, .03f), .45f), kind);

        if (kind == 1) Pine(root, bark, barkLight, canopy, canopyLight, canopyDark, lean);
        else if (kind == 2) Palm(root, bark, barkLight, canopy, canopyLight, lean);
        else Broadleaf(root, bark, barkLight, canopy, canopyLight, canopyDark, lean);
    }
}
