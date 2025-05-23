using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace DirectorySite.Models;

    public class RecoveryAccountFile
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("filePath")]
        public string? FilePath { get; set; }

        [JsonPropertyName("fileType")]
        public string? FileType { get; set; }

        [JsonPropertyName("fileUrl")]
        public string? FileUrl { get; set; }

        [JsonPropertyName("fileSize")]
        public int? FileSize { get; set; }

        [JsonPropertyName("documentTypeId")]
        public int? DocumentTypeId { get; set; }

        [JsonPropertyName("documentTypeName")]
        public string? DocumentTypeName { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("deletedAt")]
        public DateTime? DeletedAt { get; set; }
    }

    public class RecoveryAccountResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("personId")]
        public string PersonId { get; set; } = default!;

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("birthDate")]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("genderName")]
        public string? GenderName { get; set; }

        [JsonPropertyName("genderId")]
        public int? GenderId { get; set; }

        [JsonPropertyName("curp")]
        public string? Curp { get; set; }

        [JsonPropertyName("nationalityName")]
        public string? NationalityName { get; set; }

        [JsonPropertyName("nationalityId")]
        public int? NationalityId { get; set; }

        [JsonPropertyName("contactEmail")]
        public string? ContactEmail { get; set; }

        [JsonPropertyName("contactEmail2")]
        public string? ContactEmail2 { get; set; }

        [JsonPropertyName("contactPhone")]
        public string? ContactPhone { get; set; }

        [JsonPropertyName("contactPhone2")]
        public string? ContactPhone2 { get; set; }

        [JsonPropertyName("requestComments")]
        public string? RequestComments { get; set; }

        [JsonPropertyName("responseComments")]
        public string? ResponseComments { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("attendingAt")]
        public DateTime? AttendingAt { get; set; }

        [JsonPropertyName("finishedAt")]
        public DateTime? FinishedAt { get; set; }

        [JsonPropertyName("deletedAt")]
        public DateTime? DeletedAt { get; set; }

        [JsonPropertyName("totalDocuments")]
        public int? TotalDocuments { get; set; }

        [JsonPropertyName("files")]
        public List<RecoveryAccountFile>? Files { get; set; }

        [JsonPropertyName("userAttended")]
        public string? UserAttended { get; set; }

        [JsonPropertyName("userAttendedId")]
        public int? UserAttendedId { get; set; }

        [JsonPropertyName("userDeleted")]
        public string? UserDeleted { get; set; }

        [JsonPropertyName("userDeletedId")]
        public int? UserDeletedId { get; set; }
        
        [JsonPropertyName("emailNotificationResponse")]
        public string? EmailNotificationResponse { get; set; }

        [JsonPropertyName("emailNotificationContent")]
        public string? EmailNotificationContent { get; set; }

        [JsonPropertyName("occupationName")]
        public string? OccupationName { get; set; }

        [JsonPropertyName("occupationId")]
        public int? OccupationId { get; set; }

        [JsonPropertyName("maritalStatusName")]
        public string? MaritalStatusName { get; set; }

        [JsonPropertyName("maritalStatusId")]
        public int? MaritalStatusId { get; set; }

        [JsonPropertyName("rfc")]
        public string? Rfc { get; set; }


        public string FullName {
            get {
                return string.Join(" ", [ Name, FirstName, LastName]);
            }
        }
        public string BirthdateFormated {
            get {
                return BirthDate.ToString("dd MMMM yyyy", new CultureInfo("es-MX"));
            }
        }

        public string Status {
            get {
                if (DeletedAt != null) {
                    return "Eliminada";
                }
                if (FinishedAt != null) {
                    return "Finalizada";
                }
                if (AttendingAt != null) {
                    return "Atendida";
                }
                return "Pendiente";
            }
        }

         public DateTime? AttendingDate {
            get {
                if (DeletedAt != null) {
                    return DeletedAt;
                }
                if (FinishedAt != null) {
                    return FinishedAt;
                }
                if (AttendingAt != null) {
                    return AttendingAt;
                }
                return null;
            }
        }
    }

