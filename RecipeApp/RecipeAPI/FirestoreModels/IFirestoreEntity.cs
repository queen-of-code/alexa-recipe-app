namespace RecipeAPI.FirestoreModels
{
    public interface IFirestoreEntity
    {
        string Id { get; set; }
        string UserId { get; set; }
        bool IsValid();
    }
}
