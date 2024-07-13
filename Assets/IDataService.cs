public interface IDataService
{
    T LoadData<T>( string RelativePath );
    bool SaveData<T>( string RelativePath, T Data );
    public string[] GetSaveNames(string RelativePath);
}