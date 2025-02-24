import { createRootRoute, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/router-devtools";
import Styles from "./root.module.css";
import { TopBar } from "../components/TopBar.tsx";
import { RestClient } from "../restClient.ts";
import { Sidebar } from "../components/Sidebar.tsx";

export const Route = createRootRoute({
    component: () => {
        const data = Route.useLoaderData();

        return (
            <>
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
            </>
        );
    },
    loader: async () => {
        const { data, response } = await RestClient.GET("/api/user");

        return {
            user:
                response.status == 200
                    ? {
                          loggedIn: true,
                          username: data!.username,
                      }
                    : {
                          loggedIn: false,
                      },
        };
    },
});
