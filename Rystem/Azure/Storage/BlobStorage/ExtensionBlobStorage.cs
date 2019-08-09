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
using Rystem.Azure.Storage;

namespace System
{
    public static partial class ExtensionBlobStorage
    {
        /// <summary>
        /// Esegue il salvataggio asincrono di un file su Blob Storage
        /// </summary>
        /// <returns>url completa del file appena salvato</returns>
        public static string Save(this IBlobStorage blob)
        {
            return blob.SaveAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Metodo per ottenere in modo asincrono il riferimento <see cref="TEntity"/> di un file presente su Blob Storage
        /// </summary>
        /// <returns>Oggetto <see cref="TBlob"/> che raccoglie tutte le proprietà del file recuperato da Blob Storage</returns>
        public static IBlobStorage Get(this IBlobStorage blob, string name = null)
        {
            return blob.GetAsync(name).ConfigureAwait(false).GetAwaiter().GetResult();
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
        public static List<IBlobStorage> List<TEntity>(this TEntity blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
            where TEntity : IBlobStorage
        {
            return blob.ListAsync(prefix, takeCount, ct).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Metodo asincrono per verificare se un file è già presente nel Blob
        /// </summary>
        /// <param name="destinationFileName">nome del file da ricercare</param>
        /// <returns>esito della ricerca</returns>
        public static bool Exists(this IBlobStorage blob)
        {
            return blob.ExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Metodo asincrono per cancellare un file presente nel Blob
        /// </summary>
        /// <returns>esito della ricerca</returns>
        public static bool Delete(this IBlobStorage blob)
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
        public static List<string> Search(this IBlobStorage blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
        {
            return blob.SearchAsync(prefix, takeCount, ct).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Controlla in modo asincrono se la sequenza di byte in ingresso esiste nel container specificato.
        /// </summary>
        /// <param name="ByteSequence">Sequenza di byte da ricercare</param>
        /// <returns>nome della prima occorrenza della ricerca e numero di file scansionati prima di trovarla</returns>
        public static (string name, int count) CheckIfByteSequenceExists(this IBlobStorage blob, byte[] byteSequence)
        {
            return blob.CheckIfByteSequenceExistsAsync(byteSequence).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
    public static partial class ExtensionBlobStorage
    {
        private static Dictionary<string, (CloudBlobContainer, BlobType, IBlobManager)> Contexts = new Dictionary<string, (CloudBlobContainer, BlobType, IBlobManager)>();
        private static object TrafficLight = new object();
        private static (CloudBlobContainer, BlobType, IBlobManager) GetContext(Type type)
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
                        CloudBlobContainer context = Client.GetContainerReference(blobConfiguration.Container ?? type.Name.ToLower());
                        context.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        Contexts.Add(type.FullName, (context, (BlobType)(int)blobConfiguration.BlobStorageType, blobConfiguration.BlobManager ?? new JsonBlobManager()));
                    }
                }
            }
            return Contexts[type.FullName];
        }
        private static async Task<ICloudBlob> GetBlobReference(CloudBlobContainer context, string destinationFileName, BlobType blobType)
        {
            ICloudBlob cloudBlob = null;
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
        public static async Task<string> SaveAsync(this IBlobStorage blob)
        {
            Type type = blob.GetType();
            (CloudBlobContainer context, BlobType blobType, IBlobManager blobManager) = GetContext(type);
            BlobValue blobValue = blobManager.Value(blob);
            ICloudBlob cloudBlob = await GetBlobReference(context, blobValue.DestinationFileName, blobType);
            using (Stream stream = blobValue.MemoryStream)
            {
                switch (blobType)
                {
                    case BlobType.BlockBlob:
                        await cloudBlob.UploadFromStreamAsync(stream);
                        break;
                    case BlobType.AppendBlob:
                        try
                        {
                            await ((CloudAppendBlob)cloudBlob).AppendFromStreamAsync(stream);
                        }
                        catch (Exception er)
                        {
                            if (er.Message == "The specified blob does not exist.")
                            {
                                await ((CloudAppendBlob)cloudBlob).CreateOrReplaceAsync();
                                await ((CloudAppendBlob)cloudBlob).AppendFromStreamAsync(stream);
                            }
                        }
                        break;
                    case BlobType.PageBlob:
                        throw new NotImplementedException("Page blob not already implemented.");
                    default:
                        throw new NotImplementedException();
                }
                string path = new UriBuilder(cloudBlob.Uri).Uri.AbsoluteUri;
                if (CheckBlobProperty())
                    await cloudBlob.SetPropertiesAsync();
                return path;
            }
            bool CheckBlobProperty()
            {
                bool changeSomethingInProperty = false;
                if (blobValue.BlobProperties != null)
                {
                    if (blobValue.BlobProperties.ContentType != cloudBlob.Properties.ContentType)
                    {
                        cloudBlob.Properties.ContentType = blobValue.BlobProperties.ContentType ?? MimeMapping.GetMimeMapping(blobValue.DestinationFileName);
                        changeSomethingInProperty = true;
                    }
                    if (blobValue.BlobProperties.CacheControl != null && blobValue.BlobProperties.CacheControl != cloudBlob.Properties.CacheControl)
                    {
                        cloudBlob.Properties.CacheControl = blobValue.BlobProperties.CacheControl;
                        changeSomethingInProperty = true;
                    }
                    if (blobValue.BlobProperties.ContentDisposition != null && blobValue.BlobProperties.ContentDisposition != cloudBlob.Properties.ContentDisposition)
                    {
                        cloudBlob.Properties.ContentDisposition = blobValue.BlobProperties.ContentDisposition;
                        changeSomethingInProperty = true;
                    }
                    if (blobValue.BlobProperties.ContentEncoding != null && blobValue.BlobProperties.ContentEncoding != cloudBlob.Properties.ContentEncoding)
                    {
                        cloudBlob.Properties.ContentEncoding = blobValue.BlobProperties.ContentEncoding;
                        changeSomethingInProperty = true;
                    }
                    if (blobValue.BlobProperties.ContentLanguage != null && blobValue.BlobProperties.ContentLanguage != cloudBlob.Properties.ContentLanguage)
                    {
                        cloudBlob.Properties.ContentLanguage = blobValue.BlobProperties.ContentLanguage;
                        changeSomethingInProperty = true;
                    }
                    if (blobValue.BlobProperties.ContentMD5 != null && blobValue.BlobProperties.ContentMD5 != cloudBlob.Properties.ContentMD5)
                    {
                        cloudBlob.Properties.ContentMD5 = blobValue.BlobProperties.ContentMD5;
                        changeSomethingInProperty = true;
                    }
                }
                return changeSomethingInProperty;
            }
        }
        /// <summary>
        /// Metodo per ottenere in modo asincrono il riferimento <see cref="TEntity"/> di un file presente su Blob Storage
        /// </summary>
        /// <returns>Oggetto <see cref="TBlob"/> che raccoglie tutte le proprietà del file recuperato da Blob Storage</returns>
        public static async Task<IBlobStorage> GetAsync(this IBlobStorage blob, string name = null)
        {
            (CloudBlobContainer context, BlobType blobType, IBlobManager blobManager) = GetContext(blob.GetType());
            ICloudBlob cloudBlob = await GetBlobReference(context, name ?? (name = blobManager.Value(blob)?.DestinationFileName), blobType);
            if (await cloudBlob.ExistsAsync())
            {
                await cloudBlob.FetchAttributesAsync();
                var fileLenght = cloudBlob.Properties.Length;
                byte[] fileByte = new byte[fileLenght];
                await cloudBlob.DownloadToByteArrayAsync(fileByte, 0);
                return blobManager.OnRetrieve(new BlobValue()
                {
                    BlobProperties = cloudBlob.Properties,
                    MemoryStream = new MemoryStream(fileByte),
                    DestinationFileName = name
                }, blob.GetType());
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
        public static async Task<List<IBlobStorage>> ListAsync<TEntity>(this TEntity blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
            where TEntity : IBlobStorage
        {
            List<IBlobStorage> items = new List<IBlobStorage>();
            BlobContinuationToken token = null;
            (CloudBlobContainer context, BlobType blobType, IBlobManager blobManager) = GetContext(blob.GetType());
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, token, new BlobRequestOptions(), new OperationContext() { });
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    if (blobItem is CloudBlobDirectory)
                        continue;
                    items.Add(await blob.GetAsync(((ICloudBlob)blobItem).Name));
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
        public static async Task<bool> ExistsAsync(this IBlobStorage blob)
        {
            (CloudBlobContainer context, BlobType blobType, IBlobManager blobManager) = GetContext(blob.GetType());
            ICloudBlob cloudBlob = await GetBlobReference(context, blobManager.Value(blob).DestinationFileName, blobType);
            return await cloudBlob.ExistsAsync();
        }
        /// <summary>
        /// Metodo asincrono per cancellare un file presente nel Blob
        /// </summary>
        /// <returns>esito della ricerca</returns>
        public static async Task<bool> DeleteAsync(this IBlobStorage blob)
        {
            (CloudBlobContainer context, BlobType blobType, IBlobManager blobManager) = GetContext(blob.GetType());
            ICloudBlob cloudBlob = await GetBlobReference(context, blobManager.Value(blob).DestinationFileName, blobType);
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
        public static async Task<List<string>> SearchAsync(this IBlobStorage blob, string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
        {
            List<string> items = new List<string>();
            BlobContinuationToken token = null;
            (CloudBlobContainer context, BlobType blobType, IBlobManager blobManager) = GetContext(blob.GetType());
            do
            {
                BlobResultSegment segment = await context.ListBlobsSegmentedAsync(prefix, token);
                token = segment.ContinuationToken;
                foreach (var blobItem in segment.Results)
                    items.Add(blobItem.StorageUri.PrimaryUri.ToString());
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null && !ct.IsCancellationRequested);
            return items;
        }
        /// <summary>
        /// Controlla in modo asincrono se la sequenza di byte in ingresso esiste nel container specificato.
        /// </summary>
        /// <param name="byteSequence">Sequenza di byte da ricercare</param>
        /// <returns>nome della prima occorrenza della ricerca e numero di file scansionati prima di trovarla</returns>
        public static async Task<(string name, int count)> CheckIfByteSequenceExistsAsync(this IBlobStorage blob, byte[] byteSequence)
        {
            string name = String.Empty;
            int count = 0;
            BlobContinuationToken token = null;
            CancellationToken ct = default(CancellationToken);
            (CloudBlobContainer context, BlobType blobType, IBlobManager blobManager) = GetContext(blob.GetType());
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
