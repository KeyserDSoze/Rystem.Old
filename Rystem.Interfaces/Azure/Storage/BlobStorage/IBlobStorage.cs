using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.Storage
{
    /// <summary>
    /// Classe astratta che verrà implementata da <see cref="BlobStorage{TBlob}"/>>
    /// </summary>
    public interface IBlob
    {
        /// <summary>
        /// Riferimento ai valori propri del Blob
        /// </summary>
        /// <returns></returns>
        BlobValue Value();

        /// <summary>
        /// Metodo che consente di effettuare delle operazioni sull'oggetto <see cref="BlobValue"/>, una volta che viene effettuata una Get su un file del Blob.
        /// </summary>
        /// <param name="blobValue"></param>
        void OnRetrieve(BlobValue blobValue);

        /// <summary>
        /// Metodo che consente di effettuare delle operazioni sull'oggetto <see cref="BlobValue"/>, una volta che viene salvato un file nel Blob
        /// </summary>
        /// <param name="blobValue"></param>
        void OnSave(BlobValue blobValue);
    }
    /// <summary>
    /// Classe che definisce le proprietà del file su cui andranno effettuate le operazioni di Blob Storage
    /// </summary>
    public class BlobValue
    {
        /// <summary>
        /// Nome del file sul Blob Storage
        /// </summary>
        public string DestinationFileName { get; set; }

        /// <summary>
        /// File espresso come <see cref="System.IO.MemoryStream"/>
        /// </summary>
        public MemoryStream MemoryStream { get; set; }

        /// <summary>
        /// Tipologia di contenuto, in base al MIME Type del file
        /// </summary>
        public string ContentType { get; set; }
    }
}
