import { createContext } from "react";

interface LoginUserContextLoggedIn {
    loggedIn: true;
    username: string;
}

interface LoginUserContextLoggedOut {
    loggedIn: false;
}

export type LoginUserContextType =
    | LoginUserContextLoggedIn
    | LoginUserContextLoggedOut;

export const LoginUserContext = createContext<LoginUserContextType>({
    loggedIn: false,
});
