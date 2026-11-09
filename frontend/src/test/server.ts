import { setupServer } from "msw/node";

// Handlers are added per test with `server.use(...)`. The backend URL falls back to this default.
export const API = "http://localhost:8080";
export const server = setupServer();
