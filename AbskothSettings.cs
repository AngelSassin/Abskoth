using Modding;

namespace Abskoth
{
    public partial class Abskoth : IGlobalSettings<CustomGlobalSaveData>
    {
        public static CustomGlobalSaveData GlobalSaveData { get; set; } = new CustomGlobalSaveData();

        public void OnLoadGlobal(CustomGlobalSaveData s)
        {
            GlobalSaveData = s ?? GlobalSaveData ?? new CustomGlobalSaveData();
        }

        public CustomGlobalSaveData OnSaveGlobal()
        {
            return GlobalSaveData;
        }
    }

    public class CustomGlobalSaveData
    {
        public int numMarkoths = 2;
        public bool withNail = true;
    }
}
