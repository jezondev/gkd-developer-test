<template>
    <main class="container-fluid p-5 ">
        <div class="row">
            <div class="col-12 col-md-6 offset-md-6"> 
                <div class="container">
                    <div class="row">
                        <div class="col-12 d-flex justify-content-center py-5">
                            <h1>What are the odds ?</h1>
                        </div>
                    </div>
                
                    <div class="row">
                        <div class="col-12 d-flex justify-content-center">
                            <div class="uploader">
                                <label for="file" class="d-flex align-items-center px-5">
                                    <span>Upload The Empire Plan Here</span>
                                    <input id="file" type="file" @change="handleFileUpload" />
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="row" v-if="report">
                        <div class="col-12">
                            <div class="result" >
                                <div class="odds p-5 d-flex justify-content-center">{{report.bestOdds}}%</div>
                            </div>
                        </div>
                    </div>

                    <div class="row" v-if="report">
                        <div class="col-12 d-flex align-items-center flex-column">
                            <div v-for="(evaluation, evalIdx) in report.bestEvaluations" :key="evalIdx" class="py-3">
                                <h2 class="font-weight-bold text-center">Option {{evalIdx + 1}}</h2>
                                <hr/>
                                <ul class="result">
                                    <li v-for="(position, positionIdx ) in evaluation.travelPositions" :key="positionIdx">
                                        <span v-if="positionIdx === 0">Departure from {{position.planet.name}}</span>
                                        <span v-else>{{position.planet.name}} on day {{position.day}}</span>
                                        <span v-if="position.bountyHunters">, expected bounty hunters</span>
                                        <span v-if="position.refuel">, refuel required</span>
                                        <span v-if="position.waiting">, waiting necessary</span>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import axios from 'axios';

const apiUri = `http://localhost:5214`;

type TravelReport = {
    doable:boolean;
    minTravelTime : number;
    maxTravelTime: number;
    bestTravelTime: number;
    bestOdds: number | null;
    bestEvaluations : TravelEvaluation[] | null;
    otherEvaluations : TravelEvaluation[] | null;
    ImpossibleEvaluations : TravelEvaluation[] | null;
};

type TravelEvaluation = {
    travelDays : number;
    odds : number;
    travelPositions: TravelPosition[];
}

type TravelPosition = {
    day: number;
    planet : { name: string };
    bountyHunters :boolean;
    refuel: boolean;
    waiting : boolean;
}

export default defineComponent({
	name: "OnBoardComputer",
	data() {
		return {
			file: "",
            report : null as TravelReport | null
		};
	},
	methods: {
		handleFileUpload(event : any) {
			this.file = event.target.files[0];
            let formData = new FormData();
            formData.append('file', this.file);
            this.report = null;
			axios.post(`${apiUri}/travel`, formData, {
                headers: {
                    "Content-Type": "multipart/form-data",
                },
            })
            .then(
                response => {
                    this.file = "";
                    this.report = response.data
                    console.log("SUCCESS!!", response);
                },
                error => {
                    this.file = "";
                    console.log("FAILURE!! :", error);
                }
            );
            
		}
	},
});
</script>

<style lang="scss" scoped>
main {
    --dark : #161616;
    --yellow : #ffc918;
    font-family: Arial, Helvetica, sans-serif;

    height: 100vh;
    max-height: 100vh;

    background : url('../assets/img/c3po.webp') no-repeat ;
    background-position : bottom -60rem left -12rem;
    
    h1 {
        font-size: 3rem;
        font-weight : bold;
        font-style: italic;
    }

    .uploader {
        position: relative;
        overflow: hidden;

        input[type="file"] {
            position: absolute;
            visibility: hidden;
            z-index: 1;
        }

        label {
            height : 3rem;
            background-color : var(--dark);
            color : var(--yellow);
            font-weight : bold;
            border-radius: 0.5rem;
            cursor: pointer;
        } 
    }

    .result {

        .odds {
            color : var(--yellow);
            font-size : 5rem;
            font-weight : bold;
        }
    }
}

@include media-breakpoint-up(md) {
    main {
        background-position : bottom -12rem left -12rem;;
    }
}
</style>
