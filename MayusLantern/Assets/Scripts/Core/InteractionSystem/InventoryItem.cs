namespace ML.InteractionSystem
{  
    using UnityEngine;
    using ML.SceneManagement;
    using ML.Utils;

    [RequireComponent(typeof(Collider))]
    public class InventoryItem : MonoBehaviour, IDataPersister
    {
        public string inventoryKey = "";
        public LayerMask layerMask;
        public bool disableOnEnter = false;

        [HideInInspector]
        new public Collider collider;

        public AudioClip clip;
        public DataSettings dataSettings;

        private void OnEnable() {
            collider = GetComponent<Collider>();
            PersistentDataManager.RegisterPersister(this);
        }

        private void OnDisable() {
            PersistentDataManager.UnregisterPersister(this);
        }

        private void Reset() {
            layerMask = LayerMask.NameToLayer("Everything");
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
            dataSettings = new DataSettings();
        }

        private void OnTriggerEnter(Collider other) {
            if(layerMask.Contains(other.gameObject)){
                var ic = other.GetComponent<InventoryController>();
                ic.AddItem(inventoryKey);
                if(disableOnEnter){
                    gameObject.SetActive(false);
                    Save();
                }

                if(clip) AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        public void Save(){
            PersistentDataManager.SetDirty(this);
        }

        private void OnDrawGizmos() {
            Gizmos.DrawIcon(transform.position, "Inventory Item", false);
        }

        public DataSettings GetDataSettings()
        {
            return dataSettings;
        }

        public void LoadData(Data data)
        {
            Data<bool> inventoryItemData = (Data<bool>)data;
            gameObject.SetActive(inventoryItemData.value);
        }

        public Data SaveData()
        {
            return new Data<bool>(gameObject.activeSelf);
        }

        public void SetDataSettigns(string dataTag, DataSettings.PersistenceType persistenceType)
        {
            dataSettings.dataTag = dataTag;
            dataSettings.persistenceType = persistenceType;
        }
    }
}