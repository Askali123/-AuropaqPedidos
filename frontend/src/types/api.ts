// Envoltorios de respuesta usados por toda la Api (docs/05-api.md §5/§6,
// Api/Common/ApiResponse.cs y Api/Common/ErrorResponse.cs en el backend).
export interface ApiResponse<T> {
  data: T;
}

export interface ApiErrorDetail {
  code: string;
  message: string;
  details: string[];
}

export interface ApiErrorResponse {
  error: ApiErrorDetail;
}
