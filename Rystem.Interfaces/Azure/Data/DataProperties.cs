using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Data
{
    public class DataProperties
    {
        //
        // Summary:
        //     The MIME content type of the blob.
        public string ContentType { get; set; }
        //
        // Summary:
        //     An MD5 hash of the blob content. This hash is used to verify the integrity of
        //     the blob during transport. When this header is specified, the storage service
        //     checks the hash that has arrived with the one that was sent. If the two hashes
        //     do not match, the operation will fail with error code 400 (Bad Request).
        public byte[] ContentHash { get; set; }
        //
        // Summary:
        //     Specifies which content encodings have been applied to the blob. This value is
        //     returned to the client when the Get Blob operation is performed on the blob resource.
        //     The client can use this value when returned to decode the blob content.
        public string ContentEncoding { get; set; }
        //
        // Summary:
        //     Specifies the natural languages used by this resource.
        public string ContentLanguage { get; set; }
        //
        // Summary:
        //     Conveys additional information about how to process the response payload, and
        //     also can be used to attach additional metadata. For example, if set to attachment,
        //     it indicates that the user-agent should not display the response, but instead
        //     show a Save As dialog with a filename other than the blob name specified.
        public string ContentDisposition { get; set; }
        //
        // Summary:
        //     Specify directives for caching mechanisms.
        public string CacheControl { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}