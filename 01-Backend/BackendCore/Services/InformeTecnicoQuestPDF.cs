using BackendCore.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace BackendCore.Services
{
    public class InformeTecnicoQuestPDF : IDocument
    {
        private readonly InformeDtoQuest _datos;

        public InformeTecnicoQuestPDF(InformeDtoQuest datos)
        {
            _datos = datos ?? throw new ArgumentNullException(nameof(datos));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("OpenSans").FontSize(11).FontColor(Colors.Black));

                // HEADER
                page.Header().Padding(0).Column(header =>
                {
                    header.Item().Padding(0).Row(row =>
                    {
                        var currentDir = Directory.GetCurrentDirectory();
                        var imagePath = Path.Combine(currentDir, "Img", "RoyalGladCabecera.png");

                        if (File.Exists(imagePath))
                        {
                            row.ConstantColumn(165)
                                .AlignMiddle()
                                .Container()
                                .Padding(0)
                                .Height(45)
                                .Image(File.ReadAllBytes(imagePath), ImageScaling.FitArea);
                        }
                        else
                        {
                            row.ConstantColumn(165)
                                .AlignMiddle()
                                .Text("Logo no disponible")
                                .AlignLeft()
                                .FontColor(Colors.Black)
                                .Bold();
                        }

                        row.ConstantColumn(10).Text("");

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignMiddle().Padding(0).Text("ROYALGLAD SPA - RUT: 77.984.315-7")
                                .FontSize(8).FontFamily("OpenSans").FontColor(Colors.Black).Bold().AlignRight();
                            col.Item().AlignMiddle().Padding(0).Text("REPARACION Y MANTENCION DE MAQUINARIA Y EQUIPOS INDUSTRIALES")
                                .FontSize(8).FontFamily("OpenSans").FontColor(Colors.Black).Bold().AlignRight();
                            col.Item().AlignMiddle().Padding(0).Text("MAIL: ROYALGLADSPA@GMAIL.COM")
                                .FontSize(8).FontFamily("OpenSans").FontColor(Colors.Black).Bold().AlignRight();
                        });
                    });

                    header.Item()
                        .Padding(0)
                        .Height(1)
                        .Background(Colors.Grey.Darken1);

                    header.Item()
                        .PaddingTop(4)
                        .AlignCenter()
                        .Text("Informe Técnico de Visita")
                        .FontFamily("OpenSans")
                        .FontSize(20)
                        .SemiBold()
                        .FontColor(Colors.Black);
                });

                // CONTENIDO
                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(150);
                            columns.RelativeColumn(1);
                        });

                        AddTableRow(table, "Cliente", _datos.Cliente);
                        AddTableRow(table, "Fecha", _datos.Fecha);
                        AddTableRow(table, "Dirección", _datos.Direccion);
                        AddTableRow(table, "Equipo", _datos.Equipo);
                        AddTableRow(table, "Horas", _datos.Horas?.ToString() ?? "0");
                        AddTableRow(table, "Estado", _datos.Estado);

                        table.Cell().ColumnSpan(2).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                        table.Cell().ColumnSpan(2).PaddingBottom(2).Text("Trabajos realizados:")
                            .SemiBold().FontColor(Colors.Black).FontFamily("OpenSans");
                        table.Cell().ColumnSpan(2).Text(_datos.Trabajos ?? string.Empty)
                            .FontColor(Colors.Black).FontFamily("OpenSans");

                        table.Cell().ColumnSpan(2).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                        table.Cell().ColumnSpan(2).PaddingBottom(2).Text("Observaciones:")
                            .SemiBold().FontColor(Colors.Black).FontFamily("OpenSans");
                        table.Cell().ColumnSpan(2).Text(_datos.Observaciones ?? string.Empty)
                            .FontColor(Colors.Black).FontFamily("OpenSans");

                        table.Cell().ColumnSpan(2).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                        table.Cell().ColumnSpan(2).PaddingBottom(2).Text("Lubricantes utilizados:")
                            .SemiBold().FontColor(Colors.Black).FontFamily("OpenSans");
                        AddTableRow(table, "Cárter", _datos.LubricanteCarter);
                        AddTableRow(table, "Transmisión", _datos.LubricanteTransmision);
                        AddTableRow(table, "Caja", _datos.LubricanteCaja);
                        AddTableRow(table, "Diferencial", _datos.LubricanteDiferencial);
                        AddTableRow(table, "Hidráulico", _datos.LubricanteHidraulico);

                        table.Cell().ColumnSpan(2).PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                        table.Cell().ColumnSpan(2).PaddingBottom(2).Text("Filtros utilizados:")
                            .SemiBold().FontColor(Colors.Black).FontFamily("OpenSans");
                        AddTableRow(table, "Motor", _datos.FiltroMotor);
                        AddTableRow(table, "Aire", _datos.FiltroAire);
                        AddTableRow(table, "Convertidor", _datos.FiltroConvertidor);
                        AddTableRow(table, "Hidráulico", _datos.FiltroHidraulico);
                        AddTableRow(table, "Respiradores", _datos.FiltroRespiradores);

                        table.Cell().ColumnSpan(2).PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                        table.Cell().ColumnSpan(2).PaddingBottom(5).Text("Condiciones y Garantía")
                            .SemiBold()
                            .FontSize(12)
                            .FontColor(Colors.Black)
                            .FontFamily("OpenSans");

                        table.Cell().ColumnSpan(2).PaddingBottom(10).Text(
                            "Garantía del Servicio:\nSe garantiza el trabajo realizado por un periodo de 30 días a contar de la fecha de ejecución del servicio, cubriendo exclusivamente defectos derivados de la intervención realizada. Esta garantía no cubre fallas por mal uso, desgaste de piezas, ni intervenciones de terceros posteriores."
                        )
                        .FontSize(10)
                        .FontColor(Colors.Black)
                        .FontFamily("OpenSans");
                    });
                });

                // FOOTER
                page.Footer().PaddingTop(20).Column(col =>
                {
                    col.Item()
                       .Row(row =>
                       {
                           // Firma del Técnico
                           row.RelativeColumn().Column(tc =>
                           {
                               tc.Item()
                                 .PaddingBottom(5)
                                 .AlignCenter()
                                 .Text("Firma del Cliente:")
                                 .SemiBold()
                                 .FontColor(Colors.Black)
                                 .FontFamily("OpenSans");

                               if (!string.IsNullOrEmpty(_datos.FirmaBase64))
                               {
                                   var firmaBytes = ImageFromBase64(_datos.FirmaBase64);
                                   tc.Item()
                                     .AlignCenter()
                                     .Container()
                                     .Height(65)
                                     .Width(140)
                                     .Image(firmaBytes);
                               }
                               else
                               {
                                   tc.Item()
                                     .AlignCenter()
                                     .Text("Firma no disponible")
                                     .Italic()
                                     .FontColor(Colors.Black)
                                     .FontFamily("OpenSans");
                               }

                               tc.Item()
                                 .AlignCenter()
                                 .Text($"Correo: {_datos.Correo ?? "No proporcionado"}")
                                 .FontSize(10)
                                 .FontColor(Colors.Black)
                                 .FontFamily("OpenSans");
                           });

                           row.ConstantColumn(40).Text("");

                           // Firma de la Empresa centrada usando ancho fijo para imagen
                           row.RelativeColumn().Column(fc =>
                           {
                               fc.Item()
                                 .PaddingBottom(5)
                                 .AlignCenter()
                                 .Text("Firma Técnico:")
                                 .SemiBold()
                                 .FontColor(Colors.Black)
                                 .FontFamily("OpenSans");

                               var firmaEmpresaPath = Path.Combine(Directory.GetCurrentDirectory(), "Img", "FirmaEmpresa-RoyalGlad.png");
                               if (File.Exists(firmaEmpresaPath))
                               {
                                   var firmaEmpresaBytes = File.ReadAllBytes(firmaEmpresaPath);
                                   fc.Item()
                                     .AlignCenter()
                                     .Container()
                                     .Width(70)
                                     .Height(55)
                                     .Image(firmaEmpresaBytes);
                               }
                               else
                               {
                                   fc.Item()
                                     .AlignCenter()
                                     .Text("Firma empresa no disponible")
                                     .Italic()
                                     .FontColor(Colors.Black)
                                     .FontFamily("OpenSans");
                               }

                               // Agregar nombre del usuario logeado debajo de la firma de la empresa
                               fc.Item()
                                 .AlignCenter()
                                 .Text(_datos.UserName ?? "Nombre no disponible")
                                 .FontSize(10)
                                 .FontColor(Colors.Black)
                                 .FontFamily("OpenSans");
                           });
                       });
                });
            });
        }

        private void AddTableRow(TableDescriptor table, string label, string value)
        {
            table.Cell().Text(label)
                .Bold().FontColor(Colors.Black).FontFamily("OpenSans");
            table.Cell().Text(value ?? string.Empty)
                .FontColor(Colors.Black).FontFamily("OpenSans");
        }

        private byte[] ImageFromBase64(string base64)
        {
            var cleanBase64 = base64.Contains(',') ? base64.Split(',')[1] : base64;
            return Convert.FromBase64String(cleanBase64);
        }
    }
}
