import Styles from "./TopBar.module.css";
import { Button } from "./Button.tsx";
import { getRouteApi } from "@tanstack/react-router";
import { Popover } from "./Popover.tsx";
import { Separator } from "./Separator.tsx";
import { useTranslation } from "react-i18next";

export function TopBar() {
    const routeApi = getRouteApi("__root__");
    const data = routeApi.useLoaderData();
    const { t } = useTranslation();

    return (
        <div className={Styles.root}>
            <div className={Styles.icon}>Gouda Logo</div>

            {data.user.loggedIn ? (
                <Popover.Root>
                    <Popover.Trigger
                        render={(props) => (
                            <Button className={Styles.userButton} {...props} />
                        )}
                    >
                        {data.user.username}
                    </Popover.Trigger>
                    <Popover.Portal>
                        <Popover.Positioner sideOffset={4} align={"end"}>
                            <Popover.Popup>
                                <div className={Styles.userMenu}>
                                    <span>{data.user.username}</span>
                                    <Separator />
                                    <Button
                                        onClick={() =>
                                            (window.location.pathname =
                                                "/api/auth/logout")
                                        }
                                    >
                                        {t("LOG_OUT")}
                                    </Button>
                                </div>
                            </Popover.Popup>
                        </Popover.Positioner>
                    </Popover.Portal>
                </Popover.Root>
            ) : (
                <Button
                    className={Styles.userButton}
                    onClick={() => (window.location.pathname = "/api/auth")}
                >
                    {t("LOG_IN")}
                </Button>
            )}
        </div>
    );
}
