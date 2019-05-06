using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Azure.Storage.BlobStorage
{
    public static partial class ExtensionBlobStorage
    {
        /// <summary>
        /// Esegue il salvataggio asincrono di un file su Blob Storage
        /// </summary>
        /// <returns>url completa del file appena salvato</returns>
        public static string Save(this IBlob blob)
        {
            return blob.SaveAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Metodo asincrono per aggiungere una stringa ad un file già presente sul Blob.
        /// Questa operazione è consentita solamente per Blob di tipo Append.
        /// </summary>
        /// <param name="destinationFileName">Nome del file da modificare</param>
        /// <param name="text">stringa da appendere al file</param>
        /// <returns>Oggetto <see cref="TBlob"/> che è stato modificato</returns>
        public static bool AppendText<TEntity>(this IBlob blob, string text)
            where TEntity : IBlob
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return blob.AppendStream(stream);
        }
        public static bool AppendStream(this IBlob blob, Stream stream)
        {
            blob.AppendStreamAsync(stream).ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
        /// <summary>
        /// Metodo per ottenere in modo asincrono il riferimento <see cref="TEntity"/> di un file presente su Blob Storage
        /// </summary>
        /// <returns>Oggetto <see cref="TBlob"/> che raccoglie tutte le proprietà del file recuperato da Blob Storage</returns>
        public static BlobValue Get(this IBlob blob, BlobValue blobValue = null)
        {
            return blob.GetAsync(blobValue).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Metodo asincrono per ottenere una lista di <see cref="TBlob"/> in base ad un filtraggio tramite prefisso.
        /// Se la stringa di prefisso è nulla verranno elencati tutti i <see cref="TBlob"/> associati ai file dentro il <see cref="Context"/> del blob.
        /// Se la stringa di prefisso non è nulla verranno elencati i <see cref="TBlob"/> i cui file associati sul blob hanno il fileName che inizia con il prefisso specificato.
        /// </summary>
        /// <param name="prefix">stringa di ricerca per il fileName all'interno del container <see cref="Context"/> del blob</param>
        /// <param name="takeCount">limite di elementi da selezionare</param>
        /// <param name="ct">Token per la cancellazione del thread</param>
        /// <returns>Lista di <see cref="TBlob"/> selezionati in base al filtro di prefix</returns>
        public static List<BlobValue> List<TEntity>(this TEntity blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
            where TEntity : IBlob
        {
            return blob.ListAsync(prefix, takeCount, ct).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Metodo asincrono per verificare se un file è già presente nel Blob
        /// </summary>
        /// <param name="destinationFileName">nome del file da ricercare</param>
        /// <returns>esito della ricerca</returns>
        public static bool Exists(this IBlob blob)
        {
            return blob.ExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Metodo asincrono per cancellare un file presente nel Blob
        /// </summary>
        /// <returns>esito della ricerca</returns>
        public static bool Delete(this IBlob blob)
        {
            blob.DeleteAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return true;
        }
        /// <summary>
        /// Metodo asincrono per ottenere una lista di url di storage del blob, in base ad un filtraggio tramite prefisso.
        /// Se la stringa di prefisso è nulla verranno elencati tutte le url dei file dentro il <see cref="Context"/> del blob.
        /// Se la stringa di prefisso non è nulla verranno elencate le url il cui fileName inizia con il prefisso specificato.
        /// </summary>
        /// <param name="prefix">stringa di ricerca per il fileName all'interno del container <see cref="Context"/> del blob</param>
        /// <param name="takeCount">limite di elementi da selezionare</param>
        /// <param name="ct">Token per la cancellazione del thread</param>
        /// <returns>Lista di url</returns>
        public static List<string> Search(this IBlob blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
        {
            return blob.SearchAsync(prefix, takeCount, ct).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Controlla in modo asincrono se la sequenza di byte in ingresso esiste nel container specificato.
        /// </summary>
        /// <param name="ByteSequence">Sequenza di byte da ricercare</param>
        /// <returns>nome della prima occorrenza della ricerca e numero di file scansionati prima di trovarla</returns>
        public static (string name, int count) CheckIfByteSequenceExists(this IBlob blob, byte[] byteSequence)
        {
            return blob.CheckIfByteSequenceExistsAsync(byteSequence).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
    public static partial class ExtensionBlobStorage
    {
        private static Dictionary<string, (CloudBlobContainer, BlobType)> Contexts = new Dictionary<string, (CloudBlobContainer, BlobType)>();
        private static object TrafficLight = new object();
        private static (CloudBlobContainer, BlobType) GetContext(Type type)
        {
            if (!Contexts.ContainsKey(type.FullName))
            {
                lock (TrafficLight)
                {
                    if (!Contexts.ContainsKey(type.FullName))
                    {
                        BlobStorageInstaller.BlobConfiguration blobConfiguration = BlobStorageInstaller.GetConnectionString(type);
                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConfiguration.ConnectionString);
                        CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer context = Client.GetContainerReference(blobConfiguration.Container);
                        context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        Contexts.Add(type.FullName, (context, (BlobType)(int)blobConfiguration.BlobStorageType));
                    }
                }
            }
            return Contexts[type.FullName];
        }
        private static ICloudBlob GetBlobReference(string destinationFileName, Type type)
        {
            ICloudBlob cloudBlob = null;
            (CloudBlobContainer context, BlobType blobType) = GetContext(type);
            switch (blobType)
            {
                default:
                case BlobType.Unspecified:
                case BlobType.BlockBlob:
                    cloudBlob = context.GetBlockBlobReference(destinationFileName);
                    break;
                case BlobType.AppendBlob:
                    cloudBlob = context.GetAppendBlobReference(destinationFileName);
                    break;
                case BlobType.PageBlob:
                    cloudBlob = context.GetPageBlobReference(destinationFileName);
                    break;
            }
            return cloudBlob;
        }
    }
    public static partial class ExtensionBlobStorage
    {
        /// <summary>
        /// Esegue il salvataggio asincrono di un file su Blob Storage
        /// </summary>
        /// <returns>url completa del file appena salvato</returns>
        public static async Task<string> SaveAsync(this IBlob blob)
        {
            Type type = blob.GetType();
            BlobValue blobValue = blob.Value();
            ICloudBlob cloudBlob = GetBlobReference(blobValue.DestinationFileName, type);
            cloudBlob.Properties.ContentType = blobValue.ContentType ?? MimeMapping.GetMimeMapping(blobValue.DestinationFileName);
            using (Stream stream = blobValue.MemoryStream)
            {
                await cloudBlob.UploadFromStreamAsync(stream);
                string path = new UriBuilder(cloudBlob.Uri).Uri.AbsoluteUri;
                await cloudBlob.SetPropertiesAsync();
                await cloudBlob.FetchAttributesAsync();
                blob.OnSave(blobValue);
                return path;
            }
        }
        /// <summary>
        /// Metodo asincrono per aggiungere una stringa ad un file già presente sul Blob.
        /// Questa operazione è consentita solamente per Blob di tipo Append.
        /// </summary>
        /// <param name="destinationFileName">Nome del file da modificare</param>
        /// <param name="text">stringa da appendere al file</param>
        /// <returns>Oggetto <see cref="TBlob"/> che è stato modificato</returns>
        public static async Task<bool> AppendTextAsync<TEntity>(this IBlob blob, string text)
            where TEntity : IBlob
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return await blob.AppendStreamAsync(stream);
        }
        public static async Task<bool> AppendStreamAsync(this IBlob blob, Stream stream)
        {
            Type type = blob.GetType();
            BlobValue blobValue = blob.Value();
            CloudAppendBlob cloudBlob = (CloudAppendBlob)GetBlobReference(blobValue.DestinationFileName, type);
            await cloudBlob.AppendFromStreamAsync(stream);
            return true;
        }
        /// <summary>
        /// Metodo per ottenere in modo asincrono il riferimento <see cref="TEntity"/> di un file presente su Blob Storage
        /// </summary>
        /// <returns>Oggetto <see cref="TBlob"/> che raccoglie tutte le proprietà del file recuperato da Blob Storage</returns>
        public static async Task<BlobValue> GetAsync(this IBlob blob, BlobValue blobValue = null)
        {
            blobValue = blobValue ?? blob.Value();
            ICloudBlob cloudBlob = GetBlobReference(blobValue.DestinationFileName, blob.GetType());
            if (await cloudBlob.ExistsAsync())
            {
                await cloudBlob.FetchAttributesAsync();
                var fileLenght = cloudBlob.Properties.Length;
                byte[] fileByte = new byte[fileLenght];
                await cloudBlob.DownloadToByteArrayAsync(fileByte, 0);
                blobValue.ContentType = cloudBlob.Properties.ContentType;
                blobValue.MemoryStream = new MemoryStream(fileByte);
                blob.OnRetrieve(blobValue);
                return blobValue;
            }
            return null;
        }
        /// <summary>
        /// Metodo asincrono per ottenere una lista di <see cref="TBlob"/> in base ad un filtraggio tramite prefisso.
        /// Se la stringa di prefisso è nulla verranno elencati tutti i <see cref="TBlob"/> associati ai file dentro il <see cref="Context"/> del blob.
        /// Se la stringa di prefisso non è nulla verranno elencati i <see cref="TBlob"/> i cui file associati sul blob hanno il fileName che inizia con il prefisso specificato.
        /// </summary>
        /// <param name="prefix">stringa di ricerca per il fileName all'interno del container <see cref="Context"/> del blob</param>
        /// <param name="takeCount">limite di elementi da selezionare</param>
        /// <param name="ct">Token per la cancellazione del thread</param>
        /// <returns>Lista di <see cref="TBlob"/> selezionati in base al filtro di prefix</returns>
        public static async Task<List<BlobValue>> ListAsync<TEntity>(this TEntity blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
            where TEntity : IBlob
        {
            List<BlobValue> items = new List<BlobValue>();
            BlobContinuationToken token = null;
            (CloudBlobContainer context, BlobType blobType) = GetContext(blob.GetType());
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync(prefix, token);
                token = segment.ContinuationToken;
                foreach (ICloudBlob blobItem in segment.Results)
                {
                    string fileName = blobItem.Name;
                    BlobValue blobValue = new BlobValue()
                    {
                        DestinationFileName = fileName
                    };
                    items.Add(await blob.GetAsync(blobValue));
                }
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null && !ct.IsCancellationRequested);
            return items;
        }
        /// <summary>
        /// Metodo asincrono per verificare se un file è già presente nel Blob
        /// </summary>
        /// <param name="destinationFileName">nome del file da ricercare</param>
        /// <returns>esito della ricerca</returns>
        public static async Task<bool> ExistsAsync(this IBlob blob)
        {
            ICloudBlob cloudBlob = GetBlobReference(blob.Value().DestinationFileName, blob.GetType());
            return await cloudBlob.ExistsAsync();
        }
        /// <summary>
        /// Metodo asincrono per cancellare un file presente nel Blob
        /// </summary>
        /// <returns>esito della ricerca</returns>
        public static async Task<bool> DeleteAsync(this IBlob blob)
        {
            ICloudBlob cloudBlob = GetBlobReference(blob.Value().DestinationFileName, blob.GetType());
            await cloudBlob.DeleteAsync();
            return true;
        }
        /// <summary>
        /// Metodo asincrono per ottenere una lista di url di storage del blob, in base ad un filtraggio tramite prefisso.
        /// Se la stringa di prefisso è nulla verranno elencati tutte le url dei file dentro il <see cref="Context"/> del blob.
        /// Se la stringa di prefisso non è nulla verranno elencate le url il cui fileName inizia con il prefisso specificato.
        /// </summary>
        /// <param name="prefix">stringa di ricerca per il fileName all'interno del container <see cref="Context"/> del blob</param>
        /// <param name="takeCount">limite di elementi da selezionare</param>
        /// <param name="ct">Token per la cancellazione del thread</param>
        /// <returns>Lista di url</returns>
        public static async Task<List<string>> SearchAsync(this IBlob blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
        {
            List<string> items = new List<string>();
            BlobContinuationToken token = null;
            (CloudBlobContainer context, BlobType blobType) = GetContext(blob.GetType());
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync(prefix, token);
                token = segment.ContinuationToken;
                foreach (var blobItem in segment.Results)
                {
                    items.Add(blobItem.StorageUri.PrimaryUri.ToString());
                }
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null && !ct.IsCancellationRequested);
            return items;
        }
        /// <summary>
        /// Controlla in modo asincrono se la sequenza di byte in ingresso esiste nel container specificato.
        /// </summary>
        /// <param name="byteSequence">Sequenza di byte da ricercare</param>
        /// <returns>nome della prima occorrenza della ricerca e numero di file scansionati prima di trovarla</returns>
        public static async Task<(string name, int count)> CheckIfByteSequenceExistsAsync(this IBlob blob, byte[] byteSequence)
        {
            string name = String.Empty;
            int count = 0;
            BlobContinuationToken token = null;
            CancellationToken ct = default(CancellationToken);
            (CloudBlobContainer context, BlobType blobType) = GetContext(blob.GetType());
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync("", token);
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    ICloudBlob cloudBlob;
                    switch (blobType)
                    {
                        case BlobType.AppendBlob:
                            cloudBlob = (CloudAppendBlob)blobItem;
                            break;
                        default:
                        case BlobType.Unspecified:
                        case BlobType.BlockBlob:
                            cloudBlob = (CloudBlockBlob)blobItem;
                            break;
                        case BlobType.PageBlob:
                            cloudBlob = (CloudPageBlob)blobItem;
                            break;
                    }
                    count++;
                    if (byteSequence.LongLength == cloudBlob.Properties.Length)
                    {
                        //controllo dimensione file in byte
                        byte[] byteBlobItem = new byte[cloudBlob.Properties.Length];
                        await cloudBlob.DownloadToByteArrayAsync(byteBlobItem, 0);
                        if (StructuralComparisons.StructuralEqualityComparer.Equals(byteSequence, byteBlobItem)) //controllo bit a bit
                        {
                            try { name = Path.GetFileName(cloudBlob.Name); }
                            catch { }
                            break;
                        }
                    }
                }
            } while (token != null && !ct.IsCancellationRequested);
            return (name, count);
        }
    }
}
