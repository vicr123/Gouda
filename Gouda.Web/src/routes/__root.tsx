import { createRootRoute, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/router-devtools";
import Styles from "./root.module.css";
import { TopBar } from "../components/TopBar.tsx";

export const Route = createRootRoute({
    component: () => (
        <>
            <div className={Styles.background} />
            <div className={Styles.root}>
                <TopBar />
                <div className={Styles.sidebar}>Sidebar</div>
                <div className={Styles.main}>
                    <Outlet />
                </div>
            </div>
            <TanStackRouterDevtools />
        </>
    ),
});
