import { createFileRoute } from "@tanstack/react-router";
import { PageLayout } from "../components/PageLayout.tsx";
import { useTranslation } from "react-i18next";
import { RestClient } from "../restClient.ts";
import { Field } from "../components/Field.tsx";
import { Select } from "../components/Select.tsx";
import Icon from "../components/Icon.tsx";
import { Fragment, useReducer } from "react";
import { components } from "../server-schema";

type GuildChannelUpdateInfo = components["schemas"]["GuildChannelUpdateInfo"];

export const Route = createFileRoute("/guilds/$guildId")({
    component: RouteComponent,
    loader: async ({ params }) => {
        const { data } = await RestClient.GET("/api/guild/{guildId}", {
            params: {
                path: {
                    guildId: params.guildId,
                },
            },
        });

        return {
            guild: data,
        };
    },
});

function RouteComponent() {
    const data = Route.useLoaderData();

    return data.guild?.present ? (
        <ServerPresent key={data.guild?.id} />
    ) : (
        <ServerNotPresent />
    );
}

function ServerPresent() {
    const { t } = useTranslation();
    const data = Route.useLoaderData();

    const [channelState, updateChannelState] = useReducer(
        (
            prevState: GuildChannelUpdateInfo,
            action: [keyof GuildChannelUpdateInfo, string | null],
        ) => {
            return {
                ...prevState,
                [action[0]]: action[1],
            };
        },
        {
            alertChannel: data.guild!.alertChannel,
            chatLogsChannel: data.guild!.chatLogsChannel,
            superpinsChannel: data.guild!.superpinsChannel,
        },
    );

    const updateChannels = async (
        channel: keyof GuildChannelUpdateInfo,
        channelId: string | null,
    ) => {
        updateChannelState([channel, channelId]);
        await RestClient.POST("/api/guild/{guildId}/channels", {
            body: { ...channelState, [channel]: channelId },
            params: {
                path: {
                    guildId: data.guild!.id,
                },
            },
        });
    };

    return (
        <PageLayout>
            <h1>{data.guild?.name}</h1>
            <ServerChannelSelection
                selectedChannelId={channelState.alertChannel ?? null}
                onChange={(channel) => updateChannels("alertChannel", channel)}
                title={t("ALERT_CHANNEL")}
                description={t("ALERT_CHANNEL_DESCRIPTION")}
            />
            <ServerChannelSelection
                selectedChannelId={channelState.chatLogsChannel ?? null}
                onChange={(channel) =>
                    updateChannels("chatLogsChannel", channel)
                }
                title={t("CHAT_LOG_CHANNEL")}
                description={t("CHAT_LOG_CHANNEL_DESCRIPTION")}
            />
            <ServerChannelSelection
                selectedChannelId={channelState.superpinsChannel ?? null}
                onChange={(channel) =>
                    updateChannels("superpinsChannel", channel)
                }
                title={t("SUPERPIN_CHANNEL")}
            />
        </PageLayout>
    );
}

function ServerNotPresent() {
    const { t } = useTranslation();
    const data = Route.useLoaderData();
    return (
        <PageLayout>
            <h1>{data.guild?.name}</h1>
            <span>{t("INVITE_REQUIRED")}</span>
        </PageLayout>
    );
}

function ServerChannelSelection({
    selectedChannelId,
    onChange,
    title,
    description,
}: {
    selectedChannelId: string | null;
    onChange: (selectedChannelId: string | null) => void;
    title: string;
    description?: string;
}) {
    const { t } = useTranslation();

    const data = Route.useLoaderData();

    const selected = {
        channel: selectedChannelId,
    };

    return (
        <Field.Root>
            <Field.Label>{title}</Field.Label>
            <Field.Control
                render={
                    <Select.Root<{ channel: string | null }>
                        value={selected}
                        onValueChange={(value) => onChange(value.channel)}
                    >
                        <Select.Trigger>
                            <Select.Value placeholder={title} />
                            <Select.Icon>
                                <Icon icon={"arrow-down"} />
                            </Select.Icon>
                        </Select.Trigger>
                        <Select.Portal>
                            <Select.Positioner>
                                <Select.Popup>
                                    <Select.Item
                                        value={
                                            selectedChannelId == null
                                                ? selected
                                                : { channel: null }
                                        }
                                        key={"null-channel"}
                                    >
                                        <Select.ItemIndicator>
                                            <Icon icon={"dialog-ok"} />
                                        </Select.ItemIndicator>
                                        <Select.ItemText>
                                            {t("DISABLED")}
                                        </Select.ItemText>
                                    </Select.Item>
                                    {[
                                        "uncategorised" as "uncategorised",
                                        ...data.guild!.categories,
                                    ].map((category) => (
                                        <Fragment
                                            key={
                                                category == "uncategorised"
                                                    ? category
                                                    : category.id
                                            }
                                        >
                                            {category != "uncategorised" && (
                                                <Select.CategoryHeader>
                                                    {category.name}
                                                </Select.CategoryHeader>
                                            )}
                                            {data
                                                .guild!.channels.filter(
                                                    (x) =>
                                                        x.parent ==
                                                        (category ==
                                                        "uncategorised"
                                                            ? null
                                                            : category.id),
                                                )
                                                .map((channel) => (
                                                    <Select.Item
                                                        value={
                                                            selectedChannelId ==
                                                            channel.id
                                                                ? selected
                                                                : {
                                                                      channel:
                                                                          channel.id,
                                                                  }
                                                        }
                                                        key={channel.id}
                                                    >
                                                        <Select.ItemIndicator>
                                                            <Icon
                                                                icon={
                                                                    "dialog-ok"
                                                                }
                                                            />
                                                        </Select.ItemIndicator>
                                                        <Select.ItemText>
                                                            #{channel.name}
                                                        </Select.ItemText>
                                                    </Select.Item>
                                                ))}
                                        </Fragment>
                                    ))}
                                </Select.Popup>
                            </Select.Positioner>
                        </Select.Portal>
                    </Select.Root>
                }
            ></Field.Control>
            {description && (
                <Field.Description>{description}</Field.Description>
            )}
        </Field.Root>
    );
}
