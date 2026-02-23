using PT.APSInstancesClassLibrary.DTOs.Entities;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class GetCertificatesResponse
    {
        public GetCertificatesResponse(X509Certificate2Collection x509Certificate2Collection)
        {
            Certificates = new List<Certificate>();
            foreach (var item in x509Certificate2Collection)
            {
                Certificates.Add(new Certificate(item));
            }
        }
        public List<Certificate> Certificates { get; set; }
    }
}
