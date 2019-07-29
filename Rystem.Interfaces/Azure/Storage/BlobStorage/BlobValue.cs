using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.Storage
{
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
        public BlobProperties BlobProperties { get; set; }
    }
}
