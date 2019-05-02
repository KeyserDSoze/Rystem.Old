using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Rystem.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Azure.Storage
{
    /// <summary>
    /// Permette l'installazione di tutte le ConnectionString su tutte le entity del progetto.
    /// E' possibile sovrascriverle una ad una chiamando <see cref="BlobStorageInstall{TBlob}"/>.
    /// </summary>
    public static class BlobStorageInstall
    {
        internal static string ConnectionString;

        /// <summary>
        /// Installa la ConnectionString su tutte le Entity del progetto.
        /// </summary>
        /// <param name="connectionString">stringa di connessione del Blob Storage</param>
        /// <example>
        /// <code>
        /// static void OnAppStartup(){
        /// #if DEBUG
        ///     BlobStorageInstall.OnStart(StagingConnectionString);
        /// #else
        ///     BlobStorageInstall.OnStart(ProductionConnectionString);
        /// #endif
        /// }
        /// </code>
        /// </example>
        public static void OnStart(string connectionString) => ConnectionString = connectionString;
    }


    /// <summary>
    /// Permette l'installazione del singolo Blob su tutto il progetto.
    /// Per installare tutto insieme in un colpo solo usare <see cref="BlobStorageInstall"/>.
    /// </summary>
    /// <typeparam name="TBlob">Classe generica per l'installazione</typeparam>
    public static class BlobStorageInstall<TBlob> where TBlob : ABlob
    {
        /// <summary>
        /// Permette di predisporre il Client per la gestione del Blob ed il Container; questo avrà come nome il nome della classe stessa.
        /// typeof(TBlob).Name.ToLower()
        /// </summary>
        /// <param name="connectionString">stringa di connessione del Blob Storage</param>
        /// <example>
        /// <code>
        /// static void OnAppStartup()
        /// {
        /// #if DEBUG
        ///     BlobStorageInstall<typeparamref name="TBlob"/>.OnStart(StagingConnectionString);
        /// #else
        ///     BlobStorageInstall<typeparamref name="TBlob"/>.OnStart(ProductionConnectionString);
        /// #endif
        /// }
        /// </code>
        /// </example>
        public static void OnStart(string connectionString)
        {
            BlobStorage<TBlob>.OnStart(connectionString);
        }


        /// <summary>
        /// Permette di predisporre il Client per la gestione del Blob ed il Container; questo avrà come nome il nome della classe stessa.
        /// typeof(TBlob).Name.ToLower()
        /// </summary>
        /// <param name="connectionString">stringa di connessione del Blob Storage</param>
        /// <example>
        /// <code>
        /// static void OnAppStartup()
        /// {
        /// #if DEBUG
        ///     BlobStorageInstall<typeparamref name="TBlob"/>.OnStartAsync(StagingConnectionString);
        /// #else
        ///     BlobStorageInstall<typeparamref name="TBlob"/>.OnStartAsync(ProductionConnectionString);
        /// #endif
        /// }
        /// </code>
        /// </example>
        public static async Task OnStartAsync(string connectionString)
        {
            await BlobStorage<TBlob>.OnStartAsync(connectionString);
        }
    }


    /// <summary>
    /// Classe astratta che verrà implementata da <see cref="BlobStorage{TBlob}"/>>
    /// </summary>
    public abstract class ABlob
    {
        /// <summary>
        /// Tipo del Blob: Block, Append, Page. Il valore di default è Block
        /// </summary>
        public virtual BlobType Type { get; protected set; } = BlobType.BlockBlob;

        /// <summary>
        /// Riferimento ai valori propri del Blob
        /// </summary>
        /// <returns></returns>
        protected abstract BlobValue Value();

        /// <summary>
        /// Metodo che consente di effettuare delle operazioni sull'oggetto <see cref="BlobValue"/>, una volta che viene effettuata una Get su un file del Blob.
        /// </summary>
        /// <param name="blobValue"></param>
        protected virtual void OnRetrieve(BlobValue blobValue) { }

        /// <summary>
        /// Metodo che consente di effettuare delle operazioni sull'oggetto <see cref="BlobValue"/>, una volta che viene salvato un file nel Blob
        /// </summary>
        /// <param name="blobValue"></param>
        protected virtual void OnSave(BlobValue blobValue) { }
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


    /// <summary>
    /// Classe parziale che definisce proprietà di utilizzo del Blob Storage, 
    /// come la <see cref="ConnectionString"/> ed un Singleton per ottenere il container del Blob <see cref="Context"/>
    /// </summary>
    /// <typeparam name="TBlob">Oggetto generico che implementa <see cref="ABlob"/></typeparam>
    public partial class BlobStorage<TBlob> where TBlob : ABlob
    {
        private ICloudBlob cloudBlob;
        /// <summary>
        /// percorso del file nel Blob Storage
        /// </summary>
        protected string path;
        private ABlob Value;

        private static string PrivateConnectionString;
        private static string ConnectionString
        {
            get
            {
                if (PrivateConnectionString == null)
                {
                    PrivateConnectionString = (string)typeof(TBlob).GetField("ConnectionString", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
                    if (PrivateConnectionString == null)
                    {
                        PrivateConnectionString = BlobStorageInstall.ConnectionString;
                    }
                }
                return PrivateConnectionString;
            }
        }

        /// <summary>
        /// Genera un'istanza vuota di un TBlob
        /// </summary>
        public BlobStorage()
        {
            this.Value = (ABlob)Activator.CreateInstance(typeof(TBlob));
        }

        /// <summary>
        /// Permette l'Injection del Blob
        /// </summary>
        /// <param name="aBlob">Oggetto di tipo <see cref="ABlob"/></param>
        public BlobStorage(ABlob aBlob)
        {
            this.Value = aBlob;
        }

        private static object containerTrafficLight = new object();
        private static CloudBlobContainer PrivateContext;
        private static CloudBlobContainer Context
        {
            get
            {
                if (PrivateContext == null)
                {
                    lock (containerTrafficLight)
                    {
                        if (PrivateContext == null)
                        {
                            OnStart(ConnectionString);
                        }
                    }
                }
                return PrivateContext;
            }
        }

        /// <summary>
        /// Conversione implicita di Blobstorage1'[TEntity] in TBlob
        /// </summary>
        public static implicit operator ABlob(BlobStorage<TBlob> blobStorage)
        {
            return blobStorage.Value;
        }
        /// <summary>
        /// Conversione implicita di TBlob in Blobstorage1'[TEntity]
        /// </summary>
        public static implicit operator BlobStorage<TBlob>(ABlob entity)
        {
            return new BlobStorage<TBlob>(entity);
        }
    }

    /// <summary>
    /// Classe parziale che definisce le principali operazioni sincrone su Blob Storage
    /// </summary>
    public partial class BlobStorage<TBlob> where TBlob : ABlob
    {
        /// <summary>
        /// Esegue il salvataggio sincrono di un file su Blob Storage
        /// </summary>
        /// <returns>url completa del file appena salvato</returns>
        public string Save() => SaveAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Metodo sincrono per aggiungere una stringa ad un file già presente sul Blob.
        /// Questa operazione è consentita solamente per Blob di tipo Append.
        /// </summary>
        /// <param name="destinationFileName">Nome del file da modificare</param>
        /// <param name="text">stringa da appendere al file</param>
        /// <returns>Oggetto <see cref="TBlob"/> che è stato modificato</returns>
        public TBlob AppendText(string destinationFileName, string text) => AppendTextAsync(destinationFileName, text).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Metodo sincrono per aggiungere uno <see cref="Stream"/> ad un file già presente sul Blob.
        /// Questa operazione è consentita solamente per Blob di tipo Append.
        /// </summary>
        /// <param name="destinationFileName">Nome del file da modificare</param>
        /// <param name="stream">Stream da appendere al file</param>
        /// <returns>Oggetto <see cref="TBlob"/> che è stato modificato</returns>
        public TBlob AppendStream(string destinationFileName, Stream stream) => AppendStreamAsync(destinationFileName, stream).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Metodo per ottenere in modo sincrono il riferimento <see cref="TBlob"/> di un file presente su Blob Storage
        /// </summary>
        /// <returns>Oggetto <see cref="TBlob"/> che raccoglie tutte le proprietà del file recuperato da Blob Storage</returns>
        public TBlob Get(string destinationFileName) => GetAsync(destinationFileName).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Metodo sincrono per ottenere una lista di <see cref="TBlob"/> in base ad un filtraggio tramite prefisso.
        /// Se la stringa di prefisso è nulla verranno elencati tutti i <see cref="TBlob"/> associati ai file dentro il <see cref="Context"/> del blob.
        /// Se la stringa di prefisso non è nulla verranno elencati i <see cref="TBlob"/> i cui file associati sul blob hanno il fileName che inizia con il prefisso specificato.
        /// </summary>
        /// <param name="prefix">stringa di ricerca per il fileName all'interno del container <see cref="Context"/> del blob</param>
        /// <param name="takeCount">limite di elementi da selezionare</param>
        /// <param name="ct">Token per la cancellazione del thread</param>
        /// <returns>Lista di <see cref="TBlob"/> selezionati in base al filtro di prefix</returns>
        public static List<TBlob> List(string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken)) => ListAsync(prefix, takeCount, ct).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Metodo sincrono per ottenere una lista di url di storage del blob, in base ad un filtraggio tramite prefisso.
        /// Se la stringa di prefisso è nulla verranno elencati tutte le url dei file dentro il <see cref="Context"/> del blob.
        /// Se la stringa di prefisso non è nulla verranno elencate le url il cui fileName inizia con il prefisso specificato.
        /// </summary>
        /// <param name="prefix">stringa di ricerca per il fileName all'interno del container <see cref="Context"/> del blob</param>
        /// <param name="takeCount">limite di elementi da selezionare</param>
        /// <param name="ct">Token per la cancellazione del thread</param>
        /// <returns>Lista di url</returns>
        public static List<string> Search(string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken)) => SearchAsync(prefix, takeCount, ct).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Controlla in modo sincrono se la sequenza di byte in ingresso esiste nel container specificato.
        /// </summary>
        /// <param name="ByteSequence">Sequenza di byte da ricercare</param>
        /// <returns>nome della prima occorrenza della ricerca e numero di file scansionati prima di trovarla</returns>
        public (string name, int count) CheckIfByteSequenceExists(byte[] ByteSequence) => CheckIfByteSequenceExistsAsync(ByteSequence).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Metodo per la cancellazione sincrona di un file presente su Blob Storage
        /// </summary>
        /// <returns>esito della cancellazione</returns>
        public bool Delete() => DeleteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Metodo sincrono per verificare se un file è già presente nel Blob
        /// </summary>
        /// <param name="destinationFileName">nome del file da ricercare</param>
        /// <returns>esito della ricerca</returns>
        public bool Exists(string destinationFileName) => ExistsAsync(destinationFileName).ConfigureAwait(false).GetAwaiter().GetResult();

        private static void GetBlobContext() => GetBlobContextAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        internal static void OnStart(string connectionString) => OnStartAsync(connectionString).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Classe parziale che definisce le principali operazioni asincrone su Blob Storage
    /// </summary>
    public partial class BlobStorage<TBlob> where TBlob : ABlob
    {
        private BlobValue BlobValue;

        /// <summary>
        /// Esegue il salvataggio asincrono di un file su Blob Storage
        /// </summary>
        /// <returns>url completa del file appena salvato</returns>
        public async Task<string> SaveAsync()
        {
            try
            {
                FetchBlobValue();
                cloudBlob = GetBlobReference();

                cloudBlob.Properties.ContentType = BlobValue.ContentType ?? MimeMapping.GetMimeMapping(BlobValue.DestinationFileName);

                using (Stream stream = BlobValue.MemoryStream)
                {
                    await cloudBlob.UploadFromStreamAsync(stream);
                    path = new UriBuilder(cloudBlob.Uri).Uri.AbsoluteUri;
                    await cloudBlob.SetPropertiesAsync();
                    await cloudBlob.FetchAttributesAsync();
                    this.Value.GetType().GetMethod("OnSave", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this.Value, new object[1] { this.BlobValue });
                    return path;
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        /// <summary>
        /// Metodo asincrono per aggiungere una stringa ad un file già presente sul Blob.
        /// Questa operazione è consentita solamente per Blob di tipo Append.
        /// </summary>
        /// <param name="destinationFileName">Nome del file da modificare</param>
        /// <param name="text">stringa da appendere al file</param>
        /// <returns>Oggetto <see cref="TBlob"/> che è stato modificato</returns>
        public async Task<TBlob> AppendTextAsync(string destinationFileName, string text)
        {
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return await AppendStreamAsync(destinationFileName, stream);
        }

        /// <summary>
        /// Metodo asincrono per aggiungere uno <see cref="Stream"/> ad un file già presente sul Blob.
        /// Questa operazione è consentita solamente per Blob di tipo Append.
        /// </summary>
        /// <param name="destinationFileName">Nome del file da modificare</param>
        /// <param name="stream">Stream da appendere al file</param>
        /// <returns>Oggetto <see cref="TBlob"/> che è stato modificato</returns>
        public async Task<TBlob> AppendStreamAsync(string destinationFileName, Stream stream)
        {
            if (this.Value.Type == BlobType.AppendBlob)
            {
                this.BlobValue = new BlobValue()
                {
                    DestinationFileName = destinationFileName
                };
                cloudBlob = GetBlobReference();

                CloudAppendBlob appendBlob = (CloudAppendBlob)cloudBlob;
                await appendBlob.AppendFromStreamAsync(stream);

                BlobStorage<TBlob> blob = (TBlob)Activator.CreateInstance(typeof(TBlob));
                TBlob fetchedBlob = await blob.GetAsync(destinationFileName);

                return fetchedBlob;
            }
            return null;
        }

        /// <summary>
        /// Metodo per ottenere in modo asincrono il riferimento <see cref="TBlob"/> di un file presente su Blob Storage
        /// </summary>
        /// <returns>Oggetto <see cref="TBlob"/> che raccoglie tutte le proprietà del file recuperato da Blob Storage</returns>
        public async Task<TBlob> GetAsync(string destinationFileName)
        {
            if (this.BlobValue == null)
            {
                this.BlobValue = new BlobValue()
                {
                    DestinationFileName = destinationFileName
                };
            }
            cloudBlob = (ICloudBlob)GetBlobReference();
            if (await cloudBlob.ExistsAsync())
            {
                await cloudBlob.FetchAttributesAsync();
                var fileLenght = cloudBlob.Properties.Length;

                byte[] fileByte = new byte[fileLenght];

                try
                {
                    await cloudBlob.DownloadToByteArrayAsync(fileByte, 0);
                    this.BlobValue.ContentType = cloudBlob.Properties.ContentType;
                    this.BlobValue.MemoryStream = new MemoryStream(fileByte);
                    this.Value.GetType().GetMethod("OnRetrieve", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this.Value, new object[1] { this.BlobValue });
                }
                catch (Exception exc)
                {
                    throw exc;
                }

                //memoryStream.Position = 0;
                return (TBlob)this.Value;
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
        public static async Task<List<TBlob>> ListAsync(string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
        {
            List<TBlob> items = new List<TBlob>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await Context.ListBlobsSegmentedAsync(prefix, token);
                token = segment.ContinuationToken;

                foreach (ICloudBlob blobItem in segment.Results)
                {
                    if (blobItem.BlobType == new BlobStorage<TBlob>().Value.Type)
                    {
                        BlobStorage<TBlob> blob = (TBlob)Activator.CreateInstance(typeof(TBlob));
                        string fileName = blobItem.Name;
                        items.Add(await blob.GetAsync(fileName));
                    }
                }
                if (takeCount != null && items.Count >= takeCount) break;
            } while (token != null && !ct.IsCancellationRequested);
            return items;
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
        public static async Task<List<string>> SearchAsync(string prefix = null, int? takeCount = null, CancellationToken ct = default(CancellationToken))
        {
            List<string> items = new List<string>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment segment = await Context.ListBlobsSegmentedAsync(prefix, token);
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
        /// <param name="ByteSequence">Sequenza di byte da ricercare</param>
        /// <returns>nome della prima occorrenza della ricerca e numero di file scansionati prima di trovarla</returns>
        public async Task<(string name, int count)> CheckIfByteSequenceExistsAsync(byte[] ByteSequence)
        {
            string name = String.Empty;
            int count = 0;
            BlobContinuationToken token = null;
            CancellationToken ct = default(CancellationToken);

            do
            {
                BlobResultSegment segment = await Context.ListBlobsSegmentedAsync("", token);
                token = segment.ContinuationToken;
                foreach (IListBlobItem blobItem in segment.Results)
                {
                    ICloudBlob cloudBlob;

                    switch (this.Value.Type)
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

                    if (ByteSequence.LongLength == cloudBlob.Properties.Length)
                    {
                        //controllo dimensione file in byte
                        byte[] byteBlobItem = new byte[cloudBlob.Properties.Length];
                        await cloudBlob.DownloadToByteArrayAsync(byteBlobItem, 0);
                        if (StructuralComparisons.StructuralEqualityComparer.Equals(ByteSequence, byteBlobItem)) //controllo bit a bit
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

        /// <summary>
        /// Metodo per la cancellazione asincrona di un file presente su Blob Storage
        /// </summary>
        /// <returns>esito della cancellazione</returns>
        public async Task<bool> DeleteAsync()
        {
            try
            {
                FetchBlobValue();
                cloudBlob = GetBlobReference();
                return await cloudBlob.DeleteIfExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Metodo asincrono per verificare se un file è già presente nel Blob
        /// </summary>
        /// <param name="destinationFileName">nome del file da ricercare</param>
        /// <returns>esito della ricerca</returns>
        public async Task<bool> ExistsAsync(string destinationFileName)
        {
            this.BlobValue = new BlobValue()
            {
                DestinationFileName = destinationFileName
            };
            cloudBlob = GetBlobReference();
            return await cloudBlob.ExistsAsync();
        }

        /// <summary>
        /// Metodo per invocare il metodo <see cref="Value"/> dell'oggetto <see cref="TBlob"/> 
        /// Tramite questa invocazione è possibile recuperare le proprietà DestinationFileName, File e ContentType dell'oggetto <see cref="TBlob"/>
        /// </summary>
        private void FetchBlobValue()
        {
            BlobValue = (BlobValue)typeof(TBlob).GetMethod("Value", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this.Value, null);
        }

        /// <summary>
        /// Metodo per recuperare la giusta tipologia di Blob Reference in base al tipo di Blob
        /// </summary>
        internal ICloudBlob GetBlobReference()
        {
            ICloudBlob cloudBlob = null;
            switch (this.Value.Type)
            {
                default:
                case BlobType.Unspecified:
                case BlobType.BlockBlob:
                    cloudBlob = Context.GetBlockBlobReference(BlobValue.DestinationFileName);
                    break;
                case BlobType.AppendBlob:
                    cloudBlob = Context.GetAppendBlobReference(BlobValue.DestinationFileName);
                    break;
                case BlobType.PageBlob:
                    cloudBlob = Context.GetPageBlobReference(BlobValue.DestinationFileName);
                    break;
            }
            return cloudBlob;
        }

        /// <summary>
        /// Metodo per recuperare in modo asincrono le credenziali di accounting del BlobStorage.
        /// Nel dettaglio è possibile ottenere le proprietà <see cref="CloudStorageAccount"/> e <see cref="CloudBlobClient"/>
        /// Il metodo consente di predisporre il Container da utilizzare, utilizzando come nome il nome della classe stessa.
        /// </summary>
        /// <returns></returns>
        private static async Task GetBlobContextAsync()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            //(string)typeof(TBlob).GetField("ContainerName", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) ?? 
            string containerName = typeof(TBlob).Name.ToLower();
            PrivateContext = Client.GetContainerReference(containerName);
            await PrivateContext.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Metodo asincrono per l'inizializzazione della stringa di connessione e la predisposizione del Context su cui lavorare
        /// </summary>
        /// <param name="connectionString">stringa di connessione del Blob Storage</param>
        /// <returns></returns>
        internal static async Task OnStartAsync(string connectionString)
        {
            PrivateConnectionString = connectionString;
            await GetBlobContextAsync();
        }
    }
}
