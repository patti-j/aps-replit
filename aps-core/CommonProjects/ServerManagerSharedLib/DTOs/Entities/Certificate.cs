using System.Security.Cryptography.X509Certificates;

namespace PT.APSInstancesClassLibrary.DTOs.Entities
{
    public class Certificate
    {
        public Certificate(X509Certificate2 item)
        {
            Name = item.FriendlyName;
            ValidFrom = item.NotBefore.ToShortDateString() + " to " + item.NotAfter.ToShortDateString();
            Issuer = item.Issuer;
            Thumbprint = item.Thumbprint;
            Subject = item.Subject;
            SubjectName = item.SubjectName.Name;
        }

        public string Name { get; set; }
        public string ValidFrom { get; set; }
        public string Issuer { get; set; }
        public string Thumbprint { get; set; }
        public string SubjectName { get; set; }
        public string Subject { get; set; }
    }
}
