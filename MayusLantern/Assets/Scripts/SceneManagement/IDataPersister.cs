namespace ML.SceneManagement
{
    /// <summmary> 
    /// When implimented, allows data to be registered from the serialized instance of DataSettings
    /// </summary>
    using System;

    public interface IDataPersister
    {
        DataSettings GetDataSettings();

        void SetDataSettigns(string dataTag, DataSettings.PersistenceType persistenceType);

        Data SaveData();

        void LoadData(Data data);
    }

    [Serializable]
    public class DataSettings
    {
        public enum PersistenceType
        {
            DoNoPersist,
            ReadOnly,
            WriteOnly,
            ReadWrite,
        }

        public string dataTag = System.Guid.NewGuid().ToString();
        public PersistenceType persistenceType = PersistenceType.ReadWrite;

        public override string ToString()
        {
            return dataTag + " " + persistenceType.ToString();
        }
    }

    // Data is a generic class used to serialized multiple types of data depending on the data being serialized, up to 5 values per piece of data
    public class Data
    {

    }

    // Single value data, ie an int or string
    public class Data<T> : Data
    {
        public T value;

        public Data(T value)
        {
            this.value = value;
        }
    }

    // Two value data, ie a 2D vector
    public class Data<T0, T1> : Data
    {
        public T0 value0;
        public T1 value1;

        public Data(T0 value0, T1 value1)
        {
            this.value0 = value0;
            this.value1 = value1;
        }
    }

    // Three value data, ie a position vector 
    public class Data<T0, T1, T2> : Data
    {
        public T0 value0;
        public T1 value1;
        public T2 value2;

        public Data(T0 value0, T1 value1, T2 value2)
        {
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
        }
    }

    // Four value data, ie a quaternion
    public class Data<T0, T1, T2, T3> : Data
    {
        public T0 value0;
        public T1 value1;
        public T2 value2;
        public T3 value3;

        public Data(T0 value0, T1 value1, T2 value2, T3 value3)
        {
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
        }
    }

    // Five value data
    public class Data<T0, T1, T2, T3, T4> : Data
    {
        public T0 value0;
        public T1 value1;
        public T2 value2;
        public T3 value3;
        public T4 value4;

        public Data(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4)
        {
            this.value0 = value0;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4; ;
        }
    }

}