import { createFileRoute } from "@tanstack/react-router";
import { PageLayout } from "../components/PageLayout.tsx";
import { Trans, useTranslation } from "react-i18next";
import { RestClient } from "../restClient.ts";
import { Field } from "../components/Field.tsx";
import { Select } from "../components/Select.tsx";
import Icon from "../components/Icon.tsx";
import { Fragment, useReducer, useState } from "react";
import { components } from "../server-schema";
import { Separator } from "../components/Separator.tsx";
import { Button } from "../components/Button.tsx";
import { Dialog } from "../components/Dialog.tsx";
import Styles from "./guilds.module.css";
import { Textarea } from "../components/Textarea.tsx";

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
    const [ticketRoomDialogOpen, setTicketRoomDialogOpen] = useState(false);

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
            <Separator />
            <p>{t("TICKETS_DESCRIPTION")}</p>
            <Button onClick={() => setTicketRoomDialogOpen(true)}>
                {t("TICKETS_CHANNEL_CREATE")}
            </Button>

            <TicketRoomDialog
                isOpen={ticketRoomDialogOpen}
                setIsOpen={setTicketRoomDialogOpen}
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

function TicketRoomDialog({
    isOpen,
    setIsOpen,
}: {
    isOpen: boolean;
    setIsOpen: (isOpen: boolean) => void;
}) {
    const { t } = useTranslation();
    const data = Route.useLoaderData();
    const [selectedChannelId, setSelectedChannelId] = useState<string | null>(
        null,
    );
    const [message, setMessage] = useState(
        "If you need to chat to staff, use the button below to open a ticket.",
    );
    const [buttonText, setButtonText] = useState("Open Ticket");

    const createTicketMessage = async () => {
        setIsOpen(false);

        await RestClient.POST("/api/guild/{guildId}/tickets", {
            params: {
                path: {
                    guildId: data.guild!.id,
                },
            },
            body: {
                channelId: selectedChannelId!,
                message: message,
                buttonText: buttonText,
            },
        });
    };

    return (
        <Dialog.Root open={isOpen}>
            <Dialog.Portal>
                <Dialog.Backdrop />
                <Dialog.Popup className={Styles.ticketPopup}>
                    <Dialog.Title>
                        {t("TICKETS_CHANNEL_CREATE_TITLE")}
                    </Dialog.Title>
                    <Dialog.Description>
                        <Trans
                            t={t}
                            i18nKey={"TICKETS_DIALOG_DESCRIPTION"}
                            components={{
                                paragraph: <p />,
                                list: <ul />,
                                listItem: <li />,
                            }}
                        >
                            <p>
                                To get started with Tickets, create a parent
                                channel to host tickets and select it from
                                below. Gouda will send a message to facilitate
                                creation of tickets in that channel.
                            </p>
                            <p>The channel should be:</p>
                            <ul>
                                <li>
                                    public and accessible to everyone who needs
                                    to create tickets
                                </li>
                                <li>
                                    locked to server members sending messages
                                </li>
                            </ul>
                            <p>
                                If you have not already done so, you should set
                                an alert channel in order to receive
                                notifications when tickets are created.
                            </p>
                        </Trans>

                        <ServerChannelSelection
                            selectedChannelId={selectedChannelId}
                            onChange={(channel) =>
                                setSelectedChannelId(channel)
                            }
                            title={t("TICKETS_DIALOG_TICKET_CHANNEL")}
                        />
                        <Field.Root>
                            <Field.Label>
                                {t("TICKETS_DIALOG_MESSAGE_TEXT")}
                            </Field.Label>
                            <Field.Control
                                required
                                placeholder="Required"
                                value={message}
                                onChange={(e) => setMessage(e.target.value)}
                                render={<Textarea />}
                            />

                            <Field.Description>
                                {t("TICKETS_DIALOG_MESSAGE_TEXT_DESCRIPTION")}
                            </Field.Description>
                        </Field.Root>
                        <Field.Root>
                            <Field.Label>
                                {t("TICKETS_DIALOG_BUTTON_TEXT")}
                            </Field.Label>
                            <Field.Control
                                required
                                placeholder="Required"
                                value={buttonText}
                                onChange={(e) => setButtonText(e.target.value)}
                            />

                            <Field.Description>
                                {t("TICKETS_DIALOG_BUTTON_TEXT_DESCRIPTION")}
                            </Field.Description>
                        </Field.Root>
                    </Dialog.Description>
                    <div className={Styles.ticketDialogButtons}>
                        <Dialog.Close
                            render={<Button onClick={() => setIsOpen(false)} />}
                        >
                            {t("CANCEL")}
                        </Dialog.Close>
                        <div style={{ flexGrow: 1 }} />
                        <Button
                            disabled={
                                selectedChannelId == null ||
                                !message ||
                                !buttonText
                            }
                            onClick={createTicketMessage}
                        >
                            {t("TICKETS_CHANNEL_CREATE_TITLE")}
                        </Button>
                    </div>
                </Dialog.Popup>
            </Dialog.Portal>
        </Dialog.Root>
    );
}
