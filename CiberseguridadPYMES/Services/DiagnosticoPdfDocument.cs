using CiberseguridadPYMES.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CiberseguridadPYMES.Services
{
    public class DiagnosticoPdfDocument : IDocument
    {
        private readonly Diagnostico _diagnostico;

        public DiagnosticoPdfDocument(Diagnostico diagnostico)
        {
            _diagnostico = diagnostico;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public DocumentSettings GetSettings()
        {
            return DocumentSettings.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.DefaultTextStyle(x =>
                      x.FontSize(10)
                     .FontColor("#263445"));

                page.Header().Element(ConstruirEncabezado);

                page.Content().PaddingVertical(20).Column(column =>
                {
                    column.Spacing(18);

                    column.Item().Element(ConstruirResumen);

                    column.Item().Element(ConstruirResultados);

                    column.Item().Element(ConstruirHallazgos);

                    column.Item().Element(ConstruirRecomendaciones);
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Informe generado por CiberseguridadPYMES  |  Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }

        private void ConstruirEncabezado(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("CIBERSEGURIDADPYMES")
                    .FontSize(17)
                    .Bold()
                    .FontColor("#13795B");

                column.Item().Text("INFORME DE DIAGNÓSTICO DE CIBERSEGURIDAD")
                    .FontSize(11)
                    .Bold()
                    .FontColor("#263445");

                column.Item().PaddingTop(8).LineHorizontal(1)
                    .LineColor("#45B99A");
            });
        }

        private void ConstruirResumen(IContainer container)
        {
            container.Border(1)
                .BorderColor("#DCE5E9")
                .Padding(15)
                .Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Text("1. RESUMEN GENERAL")
                        .FontSize(12)
                        .Bold()
                        .FontColor("#13795B");

                    column.Item().Text(
                        $"Identificador del diagnóstico: {_diagnostico.Id}");

                    column.Item().Text(
                        $"Evaluación: {_diagnostico.EvaluacionId}");

                    column.Item().Text(
                        $"Formulario: {_diagnostico.Evaluacion?.Formulario?.Nombre ?? "No disponible"}");

                    column.Item().Text(
                        $"Fecha de generación: {_diagnostico.FechaGeneracion:dd/MM/yyyy HH:mm}");

                    column.Item().PaddingTop(5)
                        .Background("#EAF7F2")
                        .Padding(12)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(info =>
                            {
                                info.Item().Text("CUMPLIMIENTO GENERAL")
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor("#13795B");

                                info.Item().Text(
                                    $"{_diagnostico.PorcentajeGeneral:N2}%")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor("#13795B");
                            });
                        });

                    column.Item().Text(_diagnostico.Resumen)
                        .FontSize(9);
                });
        }

        private void ConstruirResultados(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item().Text("2. RESULTADOS POR DIMENSIÓN")
                    .FontSize(12)
                    .Bold()
                    .FontColor("#13795B");

                if (!_diagnostico.ResultadosDimension.Any())
                {
                    column.Item().Text(
                        "No hay resultados por dimensión registrados.");
                    return;
                }

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        CeldaEncabezado(header.Cell(), "Dimensión");
                        CeldaEncabezado(header.Cell(), "Aplicables");
                        CeldaEncabezado(header.Cell(), "Cumplidas");
                        CeldaEncabezado(header.Cell(), "Cumplimiento");
                    });

                    foreach (var resultado in _diagnostico.ResultadosDimension)
                    {
                        table.Cell().Element(Celda).Text(
                            resultado.Dimension?.Nombre ?? "Sin dimensión");

                        table.Cell().Element(Celda).Text(
                            resultado.PreguntasAplicables.ToString());

                        table.Cell().Element(Celda).Text(
                            resultado.PreguntasCumplidas.ToString());

                        table.Cell().Element(Celda).Text(
                            $"{resultado.PorcentajeCumplimiento:N2}%");
                    }
                });
            });
        }

        private void ConstruirHallazgos(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item().Text("3. HALLAZGOS IDENTIFICADOS")
                    .FontSize(12)
                    .Bold()
                    .FontColor("#13795B");

                if (!_diagnostico.Hallazgos.Any())
                {
                    column.Item().Text(
                        "No se identificaron hallazgos en esta evaluación.");
                    return;
                }

                foreach (var hallazgo in _diagnostico.Hallazgos)
                {
                    column.Item()
                        .Border(1)
                        .BorderColor("#E2E8F0")
                        .Padding(10)
                        .Column(detalle =>
                        {
                            detalle.Spacing(5);

                            detalle.Item().Text(
                                $"Prioridad: {hallazgo.Prioridad}")
                                .Bold()
                                .FontColor(
                                    hallazgo.Prioridad == "Alta"
                                        ? "#C0392B"
                                        : hallazgo.Prioridad == "Media"
                                            ? "#B7791F"
                                            : "#13795B");

                            detalle.Item().Text(
                                hallazgo.Pregunta?.Dimension?.Nombre
                                ?? "Dimensión no disponible")
                                .FontSize(8)
                                .FontColor("#64748B");

                            detalle.Item().Text(hallazgo.Descripcion);
                        });
                }
            });
        }

        private void ConstruirRecomendaciones(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                column.Item().Text("4. RECOMENDACIONES")
                    .FontSize(12)
                    .Bold()
                    .FontColor("#13795B");

                var recomendaciones = _diagnostico.Hallazgos
                    .SelectMany(h => h.Pregunta?.Recomendaciones
                        .Where(r => r.Activa)
                        ?? Enumerable.Empty<Recomendacion>())
                    .GroupBy(r => r.Id)
                    .Select(g => g.First())
                    .ToList();

                if (!recomendaciones.Any())
                {
                    column.Item().Text(
                        "No hay recomendaciones activas asociadas a los hallazgos.");
                    return;
                }

                foreach (var recomendacion in recomendaciones)
                {
                    column.Item()
                        .BorderLeft(3)
                        .BorderColor("#45B99A")
                        .PaddingLeft(10)
                        .PaddingVertical(5)
                        .Column(detalle =>
                        {
                            detalle.Spacing(4);

                            detalle.Item().Text(recomendacion.Titulo)
                                .Bold()
                                .FontColor("#263445");

                            detalle.Item().Text(recomendacion.Descripcion);

                            detalle.Item().Text(
                                $"Prioridad: {recomendacion.Prioridad}")
                                .FontSize(8)
                                .FontColor("#64748B");
                        });
                }
            });
        }

        private static void CeldaEncabezado(
            IContainer container, string texto)
        {
            container
                .Background("#13795B")
                .Padding(7)
                .Text(texto)
                .FontSize(8)
                .Bold()
                .FontColor("#FFFFFF");
        }

        private static IContainer Celda(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor("#E2E8F0")
                .Padding(7);
        }
    }
}