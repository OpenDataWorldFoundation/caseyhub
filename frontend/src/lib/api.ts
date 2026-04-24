const FALLBACK_API_BASE_URL = "http://localhost:8080";

export const API_BASE_URL =
  process.env.PUBLIC_API_URL?.trim() || FALLBACK_API_BASE_URL;

export const buildApiUrl = (path: string) => {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  console.log("Path: " + normalizedPath);
  console.log("WHole Path: " + API_BASE_URL + normalizedPath);

  return `${API_BASE_URL}${normalizedPath}`;
};
