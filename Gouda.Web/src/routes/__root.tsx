import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
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
            <LoginUserContext value={data.user}>
                <div
                    className={
                        data.user.loggedIn ? Styles.root : Styles.rootLoggedOut
                    }
                >
                    <div className={Styles.background} />
                    <div className={Styles.backgroundFilter} />
                    <TopBar />
                    {data.user.loggedIn && <Sidebar />}
                    <div className={Styles.main}>
                        <Outlet />
                    </div>
                </div>
                <TanStackRouterDevtools />
            </LoginUserContext>
        );
    },
    loader: async () => {
        const { data, response } = await RestClient.GET("/api/user");

        return {
            user:
                response.status == 200
                    ? ({
                          loggedIn: true,
                          username: data!.username,
                      } satisfies LoginUserContextType)
                    : ({
                          loggedIn: false,
                      } satisfies LoginUserContextType),
        };
    },
});
