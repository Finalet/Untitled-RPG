using UnityEngine; 

namespace MalbersAnimations.Scriptables
{
    ///<summary> Store a list of Materials</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Collections/Material Set", order = 1000)]
    public class MaterialListVar : RuntimeCollection<Material> {}
}