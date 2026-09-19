public interface IFieldEncryptionService
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}
