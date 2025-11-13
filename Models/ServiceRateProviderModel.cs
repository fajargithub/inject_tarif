using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectServiceWorker.Models
{
    //public class InjectServiceOCRModel
    //{ 
    //    public string? file_upload_reff { get; set; }
    //    public string? group_tarif { get; set; }
    //    public string? sub_tarif { get; set; }
    //    public string? nm_tarif { get; set; }
    //    public decimal? hg_jua { get; set; }
    //    public string? provider_code { get; set; }
    //    public string? kd_holding { get; set; }
    //    public string? item_id { get; set; }
    //    public decimal? disc { get; set; }
    //    public decimal? disc_rp { get; set; }
    //    public string? kd_tarif_pro { get; set; }
    //    public string? nm_tarif_pro { get; set; }
    //    public string? kd_tarif_payor { get; set; }
    //    public string? nm_tarif_payor { get; set; }
    //    public DateTime? effective_date { get; set; }
    //    public string? kd_tarif { get; set; }
    //}

    public class InjectServiceOCRModel
    {
        public string? payor_code_mc { get; set; }
        public string? payor_code_indemnity { get; set; }
        public string? provider_service_code { get; set; }
        public string? provider_code { get; set; }
        public string? group_service { get; set; }
        public string? sub_service { get; set; }
        public string? service_name { get; set; }
        public string? service_class { get; set; }
        public decimal? price { get; set; }
        public decimal? fixed_price { get; set; }
        public decimal? disc { get; set; }
        public decimal? disc_amount { get; set; }
        public DateTime? effective_date { get; set; }
        public string? agreement_type_id { get; set; }
        public string? admedika_detail_service_code { get; set; }
        public string? upload_reff { get; set; }

    }

    public class ServiceRateProviderModel
    {
        public string? kd_tarif { get; set; }
        public string? nm_tarif { get; set; }
        public Decimal? hg_jua { get; set; }
        public string? provider_code { get; set; }
        public string? item_id { get; set; }
        public string? provider_name { get; set; }
        public string? nm_item { get; set; }
        public string? nm_plan { get; set; }
        public string? mod_by { get; set; }
        public int? createdby { get; set; }
        public DateTime? createddate { get; set; }
        public int? modby { get; set; }
        public DateTime? moddate { get; set; }
        public string? kd_holding { get; set; }
        public string? nm_holding { get; set; }
        public string? kd_tarif_payor { get; set; }
        public string? nm_tarif_payor { get; set; }
        public string? kd_tarif_pro { get; set; }
        public string? nm_tarif_pro { get; set; }
        public DateTime? efective_date { get; set; }
        public Decimal? hg_jua_new { get; set; }
        public Decimal? disc { get; set; }
        public Decimal? disc_rp { get; set; }
        public string? agreement_type_desc { get; set; }
        public string? agreement_type_id { get; set; }
        public string? agreement_type { get; set; }
        public Decimal? max { get; set; }
        public Decimal? percentage { get; set; }
        public Decimal? ari_disc_rp_limit { get; set; }
        public string? st { get; set; }
        public DateTime? processeddate { get; set; }
        public string? copy_to { get; set; }
        public int? errornumber { get; set; }
        public string? messagestring { get; set; }
    }
}
