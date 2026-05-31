{{/*
Expand the name of the chart.
*/}}
{{- define "jukebox-data-manager-grpc.name" -}}
{{- .Chart.Name | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "jukebox-data-manager-grpc.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := .Chart.Name }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "jukebox-data-manager-grpc.labels" -}}
helm.sh/chart: {{ include "jukebox-data-manager-grpc.name" . }}-{{ .Chart.Version }}
{{ include "jukebox-data-manager-grpc.selectorLabels" . }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "jukebox-data-manager-grpc.selectorLabels" -}}
app.kubernetes.io/name: {{ include "jukebox-data-manager-grpc.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}
