import CountdownTimer from "@/app/auctions/CountdownTimer"
import CarImage from "@/app/auctions/CarImage"
import { Auction } from "@/app/types";
import Link from "next/link";

type Props = {
    auction: Auction
}

export default function AuctionCard({ auction }: Props) {

    return (
        <Link href={`/auctions/details/${auction.id}`} className="group">
            <div className="relative w-full bg-gray-200 aspect-video rounded-lg overflow-hidden">
                <CarImage auction={auction} />
                <div className="absolute bottom-2 left-2">
                    <CountdownTimer auctionEnd={auction.auctionEnd} />
                </div>
            </div>
            <div className="flex justify-between items-center mt-4">
                <h3 className="text-gray-700">{auction.make} {auction.model}</h3>
                <p className="font-semibold text-sm">{auction.year}</p>
            </div>
        </Link>
    )
}