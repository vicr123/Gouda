import { StrictMode } from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider } from "@tanstack/react-router";
import i18n from "i18next";
import HttpApi, { HttpBackendOptions } from "i18next-http-backend";
import "./index.css";
import { initReactI18next } from "react-i18next";
import { router } from "./router.ts";

i18n.use(initReactI18next)
    .use(HttpApi)
    .init<HttpBackendOptions>({
        lng: "en",
        fallbackLng: "en",
        backend: {
            loadPath: "/translations/{{lng}}/{{ns}}.json",
        },
    });

// Render the app
const rootElement = document.getElementById("root")!;
if (!rootElement.innerHTML) {
    const root = ReactDOM.createRoot(rootElement);
    root.render(
        <StrictMode>
            <RouterProvider router={router} />
        </StrictMode>,
    );
}
