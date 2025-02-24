import { createFileRoute } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { PageLayout } from "../components/PageLayout.tsx";
import { RestClient } from "../restClient.ts";
import { Field } from "../components/Field.tsx";
import { Select } from "../components/Select.tsx";
import Icon from "../components/Icon.tsx";

export const Route = createFileRoute("/settings")({
    component: RouteComponent,
    loader: async () => {
        const { data } = await RestClient.GET("/api/usersettings");

        return data!;
    },
});

function RouteComponent() {
    const { t } = useTranslation();
    const userSettings = Route.useLoaderData();

    const changeLanguage = async (value: string) => {
        await RestClient.POST("/api/usersettings/locale", {
            body: value,
        });
    };

    // HACK: Work around bugs in base-ui
    const userLocale = {
        locale: userSettings.locale,
    };

    return (
        <PageLayout>
            <h1>{t("USER_SETTINGS")}</h1>
            <Field.Root>
                <Field.Label>{t("LOCALE")}</Field.Label>
                <Field.Control
                    render={
                        <Select.Root<{ locale: string }>
                            value={userLocale}
                            onValueChange={(value) =>
                                changeLanguage(value.locale)
                            }
                        >
                            <Select.Trigger>
                                <Select.Value placeholder={t("LOCALE")} />
                                <Select.Icon>
                                    <Icon icon={"arrow-down"} />
                                </Select.Icon>
                            </Select.Trigger>
                            <Select.Portal>
                                <Select.Positioner>
                                    <Select.Popup>
                                        {userSettings.availableLocales.map(
                                            (locale) => (
                                                <Select.Item
                                                    value={
                                                        userLocale.locale ==
                                                        locale
                                                            ? userLocale
                                                            : { locale: locale }
                                                    }
                                                    key={locale}
                                                >
                                                    <Select.ItemIndicator>
                                                        <Icon
                                                            icon={"dialog-ok"}
                                                        />
                                                    </Select.ItemIndicator>
                                                    <Select.ItemText>
                                                        {new Intl.DisplayNames(
                                                            locale,
                                                            {
                                                                type: "language",
                                                            },
                                                        ).of(locale)}
                                                    </Select.ItemText>
                                                </Select.Item>
                                            ),
                                        )}
                                    </Select.Popup>
                                </Select.Positioner>
                            </Select.Portal>
                        </Select.Root>
                    }
                ></Field.Control>
                <Field.Description>{t("LOCALE_WARNING")}</Field.Description>
            </Field.Root>
        </PageLayout>
    );
}
