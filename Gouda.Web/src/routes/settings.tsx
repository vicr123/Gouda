import { createFileRoute, useRouter } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { PageLayout } from "../components/PageLayout.tsx";
import { RestClient } from "../restClient.ts";
import { Field } from "../components/Field.tsx";
import { Select } from "../components/Select.tsx";
import Icon from "../components/Icon.tsx";
import { Button } from "../components/Button.tsx";
import Styles from "./Settings.module.css";
import { Dialog } from "../components/Dialog.tsx";
import { useEffect, useRef, useState } from "react";
import { MapContainer, Marker, TileLayer } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { MapRef } from "react-leaflet/MapContainer";
import { Input } from "../components/Input.tsx";
import i18n from "i18next";

export const Route = createFileRoute("/settings")({
    component: RouteComponent,
    loader: async () => {
        const { data } = await RestClient.GET("/api/usersettings");

        return data!;
    },
});

function humanReadableLanguage(locale: string, selectedLanguage: string) {
    try {
        let parts = locale.split("-");

        let readableParts = [];
        let language = parts.shift();
        let script = parts.shift();
        let country = parts.shift();

        if (language)
            readableParts.push(
                new Intl.DisplayNames([selectedLanguage], {
                    type: "language",
                }).of(language),
            );

        if (script) {
            //Ensure this is actually a script
            try {
                readableParts.push(
                    `(${new Intl.DisplayNames([selectedLanguage], { type: "script" }).of(script)})`,
                );
            } catch {
                //Probably a country then
                country = script;
            }
        }

        if (country)
            readableParts.push(
                `(${new Intl.DisplayNames([selectedLanguage], { type: "region" }).of(country)})`,
            );

        return readableParts.join(" ");
    } catch {
        return locale;
    }
}

function RouteComponent() {
    const { t } = useTranslation();
    const [locationDialogOpen, setLocationDialogOpen] = useState(false);
    const userSettings = Route.useLoaderData();
    const router = useRouter();

    const [userLocation, setUserLocation] = useState(userSettings.location);

    const changeLanguage = async (value: string) => {
        await RestClient.POST("/api/usersettings/locale", {
            body: value,
        });
    };

    const clearLanguage = async () => {
        await RestClient.DELETE("/api/usersettings/locale");
        await router.invalidate();
    };

    const setLocation = async (latitude: number, longitude: number) => {
        setUserLocation({ latitude, longitude });
        await RestClient.POST("/api/usersettings/location", {
            body: {
                latitude,
                longitude,
            },
        });
        await router.invalidate();
    };

    const clearLocation = async () => {
        setUserLocation(null);
        await RestClient.DELETE("/api/usersettings/location");
        await router.invalidate();
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
                        <Select.Root<{ locale: string | null }>
                            value={userLocale}
                            onValueChange={(value) => {
                                if (value.locale) {
                                    void changeLanguage(value.locale);
                                } else {
                                    void clearLanguage();
                                }
                            }}
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
                                        <Select.Item
                                            value={
                                                userLocale.locale == null
                                                    ? userLocale
                                                    : { locale: null }
                                            }
                                            key={"null-locale"}
                                        >
                                            <Select.ItemIndicator>
                                                <Icon icon={"dialog-ok"} />
                                            </Select.ItemIndicator>
                                            <Select.ItemText>
                                                {t("USE_DISCORD_LANGUAGE")}
                                            </Select.ItemText>
                                        </Select.Item>
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
                                                        {humanReadableLanguage(
                                                            locale,
                                                            i18n.language,
                                                        )}
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
            <Field.Root>
                <Field.Label>{t("LOCATION")}</Field.Label>
                <Field.Control
                    render={
                        <Button
                            className={Styles.locationButton}
                            onClick={() => setLocationDialogOpen(true)}
                        >
                            {userLocation
                                ? `${userLocation.latitude}°, ${userLocation.longitude}°`
                                : t("LOCATION_SET_PROMPT")}
                        </Button>
                    }
                ></Field.Control>
            </Field.Root>
            <SetLocationDialog
                isOpen={locationDialogOpen}
                setIsOpen={setLocationDialogOpen}
                location={userLocation ?? undefined}
                onSetLocation={setLocation}
                onClearLocation={clearLocation}
            />
        </PageLayout>
    );
}

function SetLocationDialog({
    isOpen,
    setIsOpen,
    location,
    onSetLocation,
    onClearLocation,
}: {
    isOpen: boolean;
    setIsOpen: (isOpen: boolean) => void;
    location?: {
        latitude: number;
        longitude: number;
    };
    onSetLocation: (latitude: number, longitude: number) => void;
    onClearLocation: () => void;
}) {
    const { t } = useTranslation();
    const map = useRef<MapRef>(null);
    const [latitude, setLatitude] = useState(
        location?.latitude.toString() ?? "",
    );
    const [longitude, setLongitude] = useState(
        location?.longitude.toString() ?? "",
    );
    const [calculatingPosition, setCalculatingPosition] = useState(false);

    useEffect(() => {
        setLatitude(location?.latitude.toString() ?? "");
        setLongitude(location?.longitude.toString() ?? "");
    }, [location]);

    const mapRefSetter = (ref: MapRef) => {
        map.current = ref;
        map.current?.on("click", (e) => {
            setLatitude(e.latlng.lat.toString());
            setLongitude(e.latlng.lng.toString());
        });
    };

    const validCoordinates =
        latitude != undefined &&
        longitude != undefined &&
        latitude != "" &&
        longitude != "" &&
        !isNaN(Number(latitude)) &&
        !isNaN(Number(longitude)) &&
        Number(latitude) < 90 &&
        Number(latitude) > -90 &&
        Number(longitude) < 180 &&
        Number(longitude) > -180;

    useEffect(() => {
        if (validCoordinates) {
            map.current?.panTo([Number(latitude), Number(longitude)]);
        }
    }, [latitude, longitude]);

    const useCurrentLocation = () => {
        setCalculatingPosition(true);
        navigator.geolocation.getCurrentPosition(
            (position) => {
                setCalculatingPosition(false);
                setLatitude(
                    (Math.round(position.coords.latitude * 2) / 2).toString(),
                );
                setLongitude(
                    (Math.round(position.coords.longitude * 2) / 2).toString(),
                );
            },
            () => {
                setCalculatingPosition(false);
            },
        );
    };

    return (
        <Dialog.Root open={isOpen}>
            <Dialog.Portal>
                <Dialog.Backdrop />
                <Dialog.Popup className={Styles.mapPopup}>
                    <Dialog.Title>{t("LOCATION_SET_TITLE")}</Dialog.Title>
                    <Dialog.Description>
                        <div className={Styles.locationWrapper}>
                            <Field.Root>
                                <Field.Label>{t("LATITUDE")}</Field.Label>
                                <Input
                                    value={latitude}
                                    onChange={(e) =>
                                        setLatitude(e.target.value)
                                    }
                                />
                            </Field.Root>
                            <Field.Root>
                                <Field.Label>{t("LONGITUDE")}</Field.Label>
                                <div
                                    style={{
                                        display: "flex",
                                    }}
                                >
                                    <Input
                                        value={longitude}
                                        onChange={(e) =>
                                            setLongitude(e.target.value)
                                        }
                                    />
                                    <Button
                                        onClick={useCurrentLocation}
                                        disabled={calculatingPosition}
                                    >
                                        <Icon icon={"gps"} />
                                    </Button>
                                </div>
                            </Field.Root>
                        </div>
                        <MapContainer
                            center={
                                location
                                    ? [location.latitude, location.longitude]
                                    : [0, 0]
                            }
                            zoom={location ? 13 : 3}
                            scrollWheelZoom={true}
                            className={Styles.mapContainer}
                            ref={mapRefSetter}
                            preferCanvas={true}
                        >
                            <TileLayer
                                attribution={
                                    import.meta.env.VITE_LEAFLET_ATTRIBUTION
                                }
                                url={import.meta.env.VITE_LEAFLET_MAP_TILE_URL}
                            />
                            {validCoordinates && (
                                <Marker
                                    position={[
                                        Number(latitude),
                                        Number(longitude),
                                    ]}
                                />
                            )}
                        </MapContainer>
                    </Dialog.Description>
                    <div className={Styles.locationButtons}>
                        {location && (
                            <Button
                                onClick={() => {
                                    onClearLocation();
                                    setIsOpen(false);
                                }}
                                className={Styles.clearLocationButton}
                            >
                                {t("CLEAR_LOCATION_BUTTON")}
                            </Button>
                        )}
                        <div style={{ flexGrow: 1 }} />
                        <Dialog.Close
                            render={<Button onClick={() => setIsOpen(false)} />}
                        >
                            {t("CLOSE")}
                        </Dialog.Close>
                        <Button
                            disabled={!validCoordinates}
                            onClick={() => {
                                onSetLocation(
                                    parseFloat(latitude!),
                                    parseFloat(longitude!),
                                );
                                setIsOpen(false);
                            }}
                        >
                            {t("SET_LOCATION_BUTTON")}
                        </Button>
                    </div>
                </Dialog.Popup>
            </Dialog.Portal>
        </Dialog.Root>
    );
}
