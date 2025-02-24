import Styles from "./Sidebar.module.css";
import { ReactNode } from "react";
import { Separator } from "./Separator.tsx";
import { Link } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

export function Sidebar() {
    const { t } = useTranslation();

    return (
        <div className={Styles.sidebar}>
            <SidebarItem to={"/"}>Gouda</SidebarItem>
            <Separator />
            <SidebarItem to={"/settings"}>{t("USER_SETTINGS")}</SidebarItem>
            <SidebarItem to={"/pins"}>{t("PORTABLE_PINS")}</SidebarItem>
            <Separator />
            <span>{t("Servers")}</span>
        </div>
    );
}

export function SidebarItem({
    children,
    to,
}: {
    children: ReactNode;
    to: string;
}) {
    return (
        <Link className={Styles.sidebarItem} to={to}>
            {children}
        </Link>
    );
}
