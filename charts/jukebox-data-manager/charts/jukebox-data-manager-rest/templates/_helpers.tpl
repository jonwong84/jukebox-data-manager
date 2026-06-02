{{/*
Expand the name of the chart.
*/}}
{{- define "jukebox-data-manager-rest.name" -}}
{{- .Chart.Name | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "jukebox-data-manager-rest.fullname" -}}
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
{{- define "jukebox-data-manager-rest.labels" -}}
helm.sh/chart: {{ include "jukebox-data-manager-rest.name" . }}-{{ .Chart.Version }}
{{ include "jukebox-data-manager-rest.selectorLabels" . }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "jukebox-data-manager-rest.selectorLabels" -}}
app.kubernetes.io/name: {{ include "jukebox-data-manager-rest.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}
