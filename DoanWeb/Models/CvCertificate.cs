using System;
using System.Collections.Generic;

namespace DoanWeb.Models;

public partial class CvCertificate
{
    public int CertId { get; set; }

    public int Cvid { get; set; }

    public string CertificateName { get; set; } = null!;

    public string? Organization { get; set; }

    public int? Year { get; set; }

    public virtual Cv Cv { get; set; } = null!;
}
