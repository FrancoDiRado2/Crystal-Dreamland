namespace Aman.Features.Title.Models
{
    public class TitleModel
    {
        // Datos que el ViewModel va a consultar
        public string GameVersion { get; private set; }
        public bool HasSaveData { get; private set; }

        public TitleModel()
        {
            // Esto después lo podrías leer de un archivo o de las PlayerPrefs
            GameVersion = "v1.0.4-Alpha"; 
            HasSaveData = CheckForSaveData();
        }

        private bool CheckForSaveData()
        {
            // Lógica simple por ahora: si existe una key de guardado, devolvemos true
            return UnityEngine.PlayerPrefs.HasKey("UserSaveData");
        }
    }
}