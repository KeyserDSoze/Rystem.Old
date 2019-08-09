using System;

namespace Rystem.Azure.Storage
{
    public interface IBlobManager
    {
        /// <summary>
        /// Riferimento ai valori propri del Blob
        /// </summary>
        /// <returns></returns>
        BlobValue Value(IBlobStorage blob);
        /// <summary>
        /// Metodo che consente di effettuare delle operazioni sull'oggetto <see cref="BlobValue"/>, una volta che viene effettuata una Get su un file del Blob.
        /// </summary>
        /// <param name="blobValue"></param>
        IBlobStorage OnRetrieve(BlobValue blobValue, Type blobStorageType);
    }
}
