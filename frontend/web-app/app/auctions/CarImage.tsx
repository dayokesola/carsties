'use client';
import { Auction } from "@/app/types";

import Image from "next/image"
import { useState } from "react";

type Props = {
    auction: Auction
}
export default function CarImage({ auction }: Props) {
    const [isLoading, setLoading] = useState(true);

    return (
        <Image
            src={auction.imageUrl}
            alt={`Image of ${auction.make} ${auction.make} in  ${auction.color}`}
            fill
            priority
            sizes='(max-width:768px) 100vw, (max-width:1200px) 50vw, 25vw'
            className={`object-cover group-hover:opacity-75 duration-700 ease-in-out ${isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'}`}
            onLoad={() => setLoading(false)}
        />
    )
}