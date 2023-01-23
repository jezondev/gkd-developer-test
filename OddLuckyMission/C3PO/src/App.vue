<template>
  <OnBoardComputer />
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import OnBoardComputer from './components/OnBoardComputer.vue';

export default defineComponent({
  name: 'App',
  components: {
    OnBoardComputer
  }
});
</script>

<style lang="scss">
#app {
    overflow: hidden;
}
</style>