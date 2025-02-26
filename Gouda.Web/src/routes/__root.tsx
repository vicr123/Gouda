import {
    Await,
    createRootRouteWithContext,
    Outlet,
} from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/router-devtools";
import Styles from "./root.module.css";
import { TopBar } from "../components/TopBar.tsx";
import { RestClient } from "../restClient.ts";
import { Sidebar } from "../components/Sidebar.tsx";
import {
    LoginUserContext,
    LoginUserContextType,
} from "../context/LoginUserContext.ts";

export const Route = createRootRouteWithContext<{
    user: LoginUserContextType;
}>()({
    component: () => {
        const data = Route.useLoaderData();

        return (
            <Await
                promise={data.user}
                fallback={<div className={Styles.background} />}
            >
                {(user) => (
                    <LoginUserContext value={user}>
                        <div
                            className={
                                user.loggedIn
                                    ? Styles.root
                                    : Styles.rootLoggedOut
                            }
                        >
                            <div className={Styles.background} />
                            <div className={Styles.backgroundFilter} />
                            <TopBar />
                            {user.loggedIn && <Sidebar />}
                            <div className={Styles.main}>
                                <Outlet />
                            </div>
                        </div>
                        <TanStackRouterDevtools />
                    </LoginUserContext>
                )}
            </Await>
        );
    },
    loader: async () => {
        return {
            user: RestClient.GET("/api/user").then(
                ({ data: userData, response: userResponse }) =>
                    userResponse.status == 200
                        ? ({
                              loggedIn: true,
                              username: userData!.username,
                          } satisfies LoginUserContextType)
                        : ({
                              loggedIn: false,
                          } satisfies LoginUserContextType),
            ),
            guild: RestClient.GET("/api/guild").then(
                (response) => response.data,
            ),
        };
    },
    staleTime: 60 * 60 * 1000,
});
