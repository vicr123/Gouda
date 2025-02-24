import createClient from "openapi-fetch";
import { paths } from "./server-schema";

export const RestClient = createClient<paths>({
    baseUrl: "/",
});
